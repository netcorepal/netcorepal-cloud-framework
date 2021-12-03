using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace ABC.Gateway.AspNetCore
{
    public static class GatewayServiceCollectionExtensions
    {
        public static IReverseProxyBuilder AddABCGateway(this IServiceCollection services)
        {

           //这里路由规则从服务注册发现获取
            var builder = services.AddReverseProxy()
                .LoadFromMemory(GetRoutes(), GetClusters());

            return builder;
        }

        private static RouteConfig[] GetRoutes()
        {
            return new[]
            {
                new RouteConfig()
                {
                    RouteId = "route1",
                    ClusterId = "cluster1",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Path = "{**catch-all}"
                    }
                },
                new RouteConfig()
                {
                    RouteId = "route2",
                    ClusterId = "cluster2",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Path = "/api/{**catch-all}"
                    }
                }
            };
        }
        private static ClusterConfig[] GetClusters()
        {
            return new[]
            {
                new ClusterConfig()
                {
                    ClusterId = "cluster1",
                    //SessionAffinity = new SessionAffinityConfig { Enabled = true, Policy = "Cookie", AffinityKeyName = ".Yarp.ReverseProxy.Affinity" },
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new DestinationConfig() { Address = "https://www.baidu.com" } }
                    }
                },
                new ClusterConfig()
                {
                    ClusterId = "cluster2",
                    //SessionAffinity = new SessionAffinityConfig { Enabled = true, Policy = "Cookie", AffinityKeyName = ".Yarp.ReverseProxy.Affinity" },
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new DestinationConfig() { Address = "https://bing.com" } }
                    }
                }
            };
        }
    }
}
