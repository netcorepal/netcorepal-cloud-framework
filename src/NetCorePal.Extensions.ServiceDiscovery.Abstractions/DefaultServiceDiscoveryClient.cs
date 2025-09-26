using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace NetCorePal.Extensions.ServiceDiscovery
{
    public class DefaultServiceDiscoveryClient : IServiceDiscoveryClient
    {
        readonly IEnumerable<IServiceDiscoveryProvider> _providers;
        IReadOnlyDictionary<string, IServiceCluster> _clusters = new Dictionary<string, IServiceCluster>();
        readonly Lock _lock = new();

        public DefaultServiceDiscoveryClient(IEnumerable<IServiceDiscoveryProvider> providers)
        {
            _providers = providers;
            Reload();
            ChangeToken.OnChange(() => new CompositeChangeToken(_providers.Select(p => p.GetReloadToken()).ToList()), UpdateSnapshot);
            _changeToken = new CancellationTokenSource();
        }



        private void UpdateSnapshot()
        {
            var oldToken = _changeToken;
            _changeToken = new CancellationTokenSource();
            Reload();
            oldToken.Cancel();
        }

        public IReadOnlyDictionary<string, IServiceCluster> GetServiceClusters()
        {
            return _clusters;
        }

        void Reload()
        {
            lock (_lock)
            {
                var newclusters = new Dictionary<string, IServiceCluster>();

                foreach (var provider in _providers)
                {

                    foreach (var item in provider.Clusters)
                    {
                        newclusters.TryAdd(item.ClusterId, item);
                    }
                }
                _clusters = newclusters;
            }
        }

        private CancellationTokenSource _changeToken;
        public IChangeToken GetReloadToken()
        {
            return new CancellationChangeToken(_changeToken.Token);
        }
    }
}