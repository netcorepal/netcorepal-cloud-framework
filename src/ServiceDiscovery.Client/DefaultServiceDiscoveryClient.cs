using Microsoft.Extensions.Primitives;

namespace NetCorePal.ServiceDiscovery.Client
{
    public class DefaultServiceDiscoveryClient : IServiceDiscoveryClient
    {
        IEnumerable<IServiceDiscoveryProvider> _providers;
        IReadOnlyDictionary<string, IServiceCluster>? _clusters;


        public DefaultServiceDiscoveryClient(IEnumerable<IServiceDiscoveryProvider> providers)
        {
            _providers = providers;

            ChangeToken.OnChange(()=>new CompositeChangeToken(_providers.Select(p => p.GetReloadToken()).ToList()), UpdateSnapshot);
            _changeToken = new CancellationTokenSource();
        }



        private void UpdateSnapshot()
        {
            var oldToken = _changeToken;
            _changeToken = new CancellationTokenSource();
            oldToken.Cancel();
        }

        public IReadOnlyDictionary<string, IServiceCluster> GetServiceClusters()
        {

            if (_clusters == null)
            {
                lock (this)
                {
                    if (_clusters == null)
                    {
                        var newclusters = new Dictionary<string, IServiceCluster>();

                        foreach (var provider in _providers)
                        {
                            newclusters.Concat(provider.Clusters.ToDictionary(p => p.ClusterId));
                        }
                        _clusters = newclusters;
                    }
                }
            }
            return _clusters;

        }

        private CancellationTokenSource _changeToken;
        public IChangeToken GetReloadToken()
        {
            return new CancellationChangeToken(_changeToken.Token);
        }
    }
}