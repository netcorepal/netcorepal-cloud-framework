using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace NetCorePal.Gateway.AspNetCore
{
    public class InMemoryConfigProvider : IProxyConfigProvider
    {
        // Marked as volatile so that updates are atomic
        private volatile InMemoryConfig _config;

        public InMemoryConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            _config = new InMemoryConfig(routes, clusters);
        }

        /// <summary>
        /// Implementation of the IProxyConfigProvider.GetConfig method to supply the current snapshot of configuration
        /// </summary>
        /// <returns>An immutable snapshot of the current configuration state</returns>
        public IProxyConfig GetConfig() => _config;

        /// <summary>
        /// Swaps the config state with a new snapshot of the configuration, then signals the change
        /// </summary>
        public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            var oldConfig = _config;
            _config = new InMemoryConfig(routes, clusters);
            oldConfig.SignalChange();
        }
    }
}