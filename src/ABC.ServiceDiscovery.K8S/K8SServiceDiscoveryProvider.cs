using ABC.ServiceDiscovery.Abstractions;
using k8s;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ABC.ServiceDiscovery.K8S
{
    /// <summary>
    /// 实现基于K8S的服务提供者
    /// </summary>
    public class K8SServiceDiscoveryProvider : IServiceDiscoveryProvider, IDisposable
    {
        private readonly ILogger<K8SServiceDiscoveryProvider>? _logger;

        private IEnumerable<RemoteServiceDescriptor> _serviceDescriptors { get; set; } = null!;

        private readonly K8SProviderOption _k8SProviderOption;

        /// <summary>
        /// k8s客户端
        /// </summary>
        private readonly IKubernetes _k8SClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public K8SServiceDiscoveryProvider(IOptions<K8SProviderOption> options,
            ILogger<K8SServiceDiscoveryProvider> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            ValidateConfig(options.Value);
            var k8sClientConfig = GetK8sClientConfiguration(options.Value);
            _k8SClient = new Kubernetes(k8sClientConfig);
            _k8SProviderOption = options.Value;
            _logger = logger;
        }

        #region 配置校验&初始化k8s客户端

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
            if (string.IsNullOrWhiteSpace(options.ServerAddress))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ServerAddress)}");
            }

            if (string.IsNullOrWhiteSpace(options.CertificateAuthorityData))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.CertificateAuthorityData)}");
            }

            if (string.IsNullOrWhiteSpace(options.ClientKeyData))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ClientKeyData)}");
            }

            if (string.IsNullOrWhiteSpace(options.ClientCertificateData))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.ClientCertificateData)}");
            }

            if (string.IsNullOrWhiteSpace(options.LabelOfSearch))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.LabelOfSearch)}");
            }

            if (string.IsNullOrWhiteSpace(options.LabelKeyOfServiceName))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.LabelKeyOfServiceName)}");
            }

            if (string.IsNullOrWhiteSpace(options.LabelKeyOfVersion))
            {
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.LabelKeyOfVersion)}");
            }
        }

        /// <summary>
        /// provider配置信息转化成k8sclient对应配置信息
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private KubernetesClientConfiguration GetK8sClientConfiguration(K8SProviderOption options)
        {
            //将option 转化为k8sclientconfig
            var defaultName = "deafult";//默认名称，用于串接集群账户数据
            var clusters = new List<Cluster>
            {
                new Cluster
                {
                    ClusterEndpoint = new ClusterEndpoint
                    {
                        CertificateAuthorityData = options.CertificateAuthorityData,
                        Server = options.ServerAddress
                    },
                    Name = defaultName
                }
            };

            var users = new List<User>
            {
                new User
                {
                    Name = defaultName,
                    UserCredentials = new UserCredentials
                    {
                        ClientKeyData = options.ClientKeyData,
                        ClientCertificateData = options.ClientCertificateData
                    }
                }
            };

            var contexts = new List<Context>
            {
                new Context
                {
                    Name = defaultName,
                    ContextDetails = new ContextDetails
                    {
                        Cluster = defaultName,
                        User = defaultName,
                    }
                }
            };


            var k8sClientConfig = new K8SConfiguration()
            {
                CurrentContext = defaultName,
                Clusters = clusters,
                Contexts = contexts,
                Users = users
            };
            return KubernetesClientConfiguration.BuildConfigFromConfigObject(k8sClientConfig);
        }

        #endregion

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

        async Task<IEnumerable<RemoteServiceDescriptor>> QueryServicesByLabelAsync(CancellationToken cancellationToken)
        {
            var k8sServiceList = await _k8SClient.ListServiceForAllNamespacesAsync(labelSelector: _k8SProviderOption.LabelOfSearch, cancellationToken: cancellationToken).ConfigureAwait(false);
            var serviceDescriptors = new List<RemoteServiceDescriptor>();
            foreach (var k8sService in k8sServiceList.Items)
            {
                var labels = k8sService.Metadata.Labels;
                //string version = labels.ContainsKey(_k8SProviderOption.LabelKeyOfVersion) ? labels[_k8SProviderOption.LabelKeyOfVersion] ?? string.Empty : string.Empty;
                serviceDescriptors.Add(new RemoteServiceDescriptor
                (
                    serviceName: labels.ContainsKey(_k8SProviderOption.LabelKeyOfServiceName) ? labels[_k8SProviderOption.LabelKeyOfServiceName] : k8sService.Metadata.Name,
                    instanceId: labels.ContainsKey(_k8SProviderOption.LabelKeyOfServiceName) ? labels[_k8SProviderOption.LabelKeyOfServiceName] : k8sService.Metadata.Name,
                    host: $"{ k8sService.Metadata.Name }.{ k8sService.Metadata.NamespaceProperty }.svc.cluster.local",
                    port: 80,
                    isSecure: false,
                    uri: new Uri($"http://{ k8sService.Metadata.Name }.{ k8sService.Metadata.NamespaceProperty }.svc.cluster.local"),
                    metadata: new Dictionary<string, string>(k8sService.Metadata.Labels)
                ));
            }
            return serviceDescriptors;
        }

        public IEnumerable<RemoteServiceDescriptor> GetServices(string serviceName)
        {
            return _serviceDescriptors.Where(x => x.ServiceName == serviceName);
        }


        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
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


    }
}