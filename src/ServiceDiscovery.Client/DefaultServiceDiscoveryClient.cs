using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace NetCorePal.ServiceDiscovery.Client
{
    public class DefaultServiceDiscoveryClient : IServiceDiscoveryClient
    {
        IEnumerable<IServiceDiscoveryProvider> _providers;
        IReadOnlyDictionary<string, IServiceCluster> _clusters = new Dictionary<string, IServiceCluster>();


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
            lock (this)
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