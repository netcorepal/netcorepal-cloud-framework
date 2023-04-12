using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NetCorePal.ServiceDiscovery;

namespace NetCorePal.ServiceDiscovery.K8S
{
    /// <summary>
    /// 实现基于K8S的服务提供者
    /// </summary>
    public class K8SServiceDiscoveryProvider : IServiceDiscoveryProvider, IHostedService, IDisposable
    {
        private readonly ILogger<K8SServiceDiscoveryProvider>? _logger;

        private IEnumerable<IDestination> _serviceDescriptors { get; set; } = null!;

        public IEnumerable<IServiceCluster> Clusters { get; init; }

        private readonly K8SProviderOption _k8SProviderOption;

        private CancellationTokenSource _cts = new CancellationTokenSource();
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
            _k8SProviderOption = options.Value;
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

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
        public async Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _serviceDescriptors = await QueryServicesByLabelAsync(cancellationToken).ConfigureAwait(false);
        }

        async Task<IEnumerable<IDestination>> QueryServicesByLabelAsync(CancellationToken cancellationToken)
        {
            var k8sServiceList = await _k8SClient.CoreV1.ListServiceForAllNamespacesAsync(labelSelector: _k8SProviderOption.LabelOfSearch, cancellationToken: cancellationToken).ConfigureAwait(false);

            var serviceDescriptors = k8sServiceList.Items.Select(k8sService =>
            {
                var labels = k8sService.Metadata.Labels;
                return new Destination
                (
                    serviceName: labels.ContainsKey(_k8SProviderOption.LabelKeyOfServiceName) ? labels[_k8SProviderOption.LabelKeyOfServiceName] : k8sService.Metadata.Name,
                    instanceId: labels.ContainsKey(_k8SProviderOption.LabelKeyOfServiceName) ? labels[_k8SProviderOption.LabelKeyOfServiceName] : k8sService.Metadata.Name,
                    address: $"http://{k8sService.Metadata.Name}.{k8sService.Metadata.NamespaceProperty}.svc.cluster.local",
                    metadata: new Dictionary<string, string>(k8sService.Metadata.Labels)
                ) as IDestination;
            });
            _lastResourceVersion = k8sServiceList.ResourceVersion();
            return serviceDescriptors ?? new List<IDestination>();
        }

        public IEnumerable<IDestination> GetServices(string serviceName)
        {
            return _serviceDescriptors.Where(x => x.ServiceName == serviceName);
        }


        public IChangeToken GetReloadToken()
        {
            return _token;
        }

        #region IDisposable

        private bool isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                _k8SClient.Dispose();
            }
            isDisposed = true;
        }

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
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    var watchWithHttpMessage = _k8SClient.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(labelSelector: _k8SProviderOption.LabelOfSearch, watch: true, resourceVersion: _lastResourceVersion, cancellationToken: cancellationToken);
                    await foreach (var (type, item) in watchWithHttpMessage.WatchAsync<V1Service, V1ServiceList>())
                    {
                        break;  //目前采用全量更新的方式，后续可以优化增量更新
                    }

                    var _oldcts = _cts;
                    _cts = new CancellationTokenSource();
                    _token = new CancellationChangeToken(_cts.Token);
                    await LoadAsync();
                    _oldcts.Cancel();
                }
                catch { }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}