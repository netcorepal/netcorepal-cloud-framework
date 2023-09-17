using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace NetCorePal.Extensions.ServiceDiscovery.K8s
{
    /// <summary>
    /// 实现基于K8S的服务发现提供者
    /// </summary>
    public class K8SServiceDiscoveryProvider : BackgroundService, IServiceDiscoveryProvider
    {
        private readonly ILogger<K8SServiceDiscoveryProvider> _logger;
        private IEnumerable<IDestination> _serviceDescriptors = new List<IDestination>();
        public IEnumerable<IServiceCluster> Clusters { get; private set; }
        private readonly K8SProviderOption _options;
        private CancellationTokenSource _cts = new();
        private IChangeToken _token;

        /// <summary>
        /// k8s客户端
        /// </summary>
        private readonly IKubernetes _k8SClient;

        private string? _lastResourceVersion;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="k8SClient"></param>
        /// <param name="logger"></param>
        public K8SServiceDiscoveryProvider(IOptions<K8SProviderOption> options,
            IKubernetes k8SClient,
            ILogger<K8SServiceDiscoveryProvider> logger)
        {
            ValidateConfig(options.Value);
            _k8SClient = k8SClient ?? throw new ArgumentNullException(nameof(k8SClient));
            _options = options.Value;
            _logger = logger;
            Clusters = new List<IServiceCluster>();
            _token = new CancellationChangeToken(_cts.Token);
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="options"></param>
        private void ValidateConfig(K8SProviderOption options)
        {
            //参数基本校验
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.LabelKeyOfServiceName))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.LabelKeyOfServiceName)}");
            }
        }

        public void Load()
        {
            var result = LoadAsync().ConfigureAwait(false);
            result.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            _serviceDescriptors = await QueryServicesByLabelAsync(cancellationToken).ConfigureAwait(false);
        }

        async Task<IEnumerable<IDestination>> QueryServicesByLabelAsync(CancellationToken cancellationToken)
        {
            var k8sServiceList = string.IsNullOrEmpty(_options.ServiceNamespace)
                ? await _k8SClient.CoreV1
                    .ListServiceForAllNamespacesAsync(labelSelector: _options.LabelOfSearch,
                        cancellationToken: cancellationToken)
                : await _k8SClient.CoreV1.ListNamespacedServiceAsync(_options.ServiceNamespace,
                    labelSelector: _options.LabelOfSearch,
                    cancellationToken: cancellationToken);

            var serviceDescriptors = new List<IDestination>();

            foreach (var service in k8sServiceList.Items)
            {
                if (TryGetService(service, out var destination))
                {
                    serviceDescriptors.Add(destination);
                }
            }

            _lastResourceVersion = k8sServiceList.ResourceVersion();

            this.Clusters = serviceDescriptors.GroupBy(x => x.ServiceName).Select(x =>
                    new ServiceCluster
                        { ClusterId = x.Key, Destinations = x.ToDictionary(p => p.InstanceId) } as IServiceCluster)
                .ToList();

            return serviceDescriptors;
        }

        public IEnumerable<IDestination> GetServices(string serviceName)
        {
            return _serviceDescriptors.Where(x => x.ServiceName == serviceName);
        }


        public IChangeToken GetReloadToken()
        {
            return _token;
        }

        bool TryGetService(V1Service service, out IDestination destination)
        {
            destination = null!;
            var labels = service.Metadata.Labels;
            if (labels == null)
            {
                return false;
            }

            string env = string.Empty;
            if (!string.IsNullOrEmpty(_options.EnvLabelKey) &&
                labels.TryGetValue(_options.EnvLabelKey, out var envLabel))
            {
                env = envLabel;
            }

            destination = new Destination
            (
                serviceName: (labels.ContainsKey(_options.LabelKeyOfServiceName)
                    ? labels[_options.LabelKeyOfServiceName]
                    : service.Metadata.Name) + env,
                instanceId: service.Metadata.Name,
                address: GetAddress(service),
                metadata: new Dictionary<string, string>(labels)
            );
            return true;
        }

        string GetAddress(V1Service service)
        {
            int port = _options.ServiceScheme == SchemeType.Http ? 80 : 443;
            if (_options.ServicePort != null)
            {
                var servicePort = service.Spec?.Ports?.FirstOrDefault(p => p.Port == _options.ServicePort);
                if (servicePort == null)
                {
                    _logger.LogWarning("Service {ServiceName} not found port {ServicePort}",
                        service.Metadata.Name,
                        _options.ServicePort);
                }
                else
                {
                    port = servicePort.Port;
                }
            }
            else if (!string.IsNullOrEmpty(_options.ServicePortName))
            {
                var servicePort = service.Spec?.Ports?.FirstOrDefault(p => p.Name == _options.ServicePortName);
                if (servicePort == null)
                {
                    _logger.LogWarning("Service {ServiceName} not found port name {ServicePortName}",
                        service.Metadata.Name,
                        _options.ServicePortName);
                }
                else
                {
                    port = servicePort.Port;
                }
            }

            bool isSchemePortMatch = (_options.ServiceScheme == SchemeType.Http && port == 80)
                                     || (_options.ServiceScheme == SchemeType.Https && port == 443);
            string portString = isSchemePortMatch ? string.Empty : (":" + port.ToString());
            var schemeName = _options.ServiceScheme == SchemeType.Http ? "http" : "https";
            return
                $"{schemeName}://{service.Metadata.Name}.{service.Metadata.NamespaceProperty}.svc.cluster.local{portString}";
        }

        #region IDisposable

        public Task RegisterAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeregisterAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region IHostedService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var watchWithHttpMessage = string.IsNullOrEmpty(_options.ServiceNamespace)
                        ? _k8SClient.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(
                            labelSelector: _options.LabelOfSearch,
                            watch: true,
                            resourceVersion: _lastResourceVersion,
                            cancellationToken: stoppingToken)
                        : _k8SClient.CoreV1.ListNamespacedServiceWithHttpMessagesAsync(
                            _options.ServiceNamespace,
                            labelSelector: _options.LabelOfSearch,
                            watch: true,
                            resourceVersion: _lastResourceVersion,
                            cancellationToken: stoppingToken);
                    await foreach (var (type, item) in watchWithHttpMessage.WatchAsync<V1Service, V1ServiceList>()
                                       .WithCancellation(stoppingToken))
                    {
                        if (type == WatchEventType.Deleted)
                        {
                        }
                        else if (type == WatchEventType.Modified || type == WatchEventType.Bookmark)
                        {
                        }
                        else if (type == WatchEventType.Added)
                        {
                        }
                        else if (type == WatchEventType.Error)
                        {
                            //重启watch
                        }

                        break; //目前采用全量更新的方式，后续可以优化增量更新
                    }

                    var oldCts = _cts;
                    _cts = new CancellationTokenSource();
                    _token = new CancellationChangeToken(_cts.Token);
                    await LoadAsync(stoppingToken);
                    oldCts.Cancel();
                }
                catch (Exception exception)
                {
                    _logger?.LogError(exception, "K8S watch error");
                }
            }
        }

        #endregion
    }
}