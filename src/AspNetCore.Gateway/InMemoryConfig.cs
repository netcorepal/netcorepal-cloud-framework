using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace NetCorePal.Gateway.AspNetCore
{
    /// <summary>
    /// Implementation of IProxyConfig which is a snapshot of the current config state. The data for this class should be immutable.
    /// </summary>
    public class InMemoryConfig : IProxyConfig
    {
        // Used to implement the change token for the state
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        /// <summary>
        /// A snapshot of the list of routes for the proxy
        /// </summary>
        public IReadOnlyList<RouteConfig> Routes { get; }

        /// <summary>
        /// A snapshot of the list of Clusters which are collections of interchangable destination endpoints
        /// </summary>
        public IReadOnlyList<ClusterConfig> Clusters { get; }

        /// <summary>
        /// Fired to indicate the the proxy state has changed, and that this snapshot is now stale
        /// </summary>
        public IChangeToken ChangeToken { get; }

        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }
}
