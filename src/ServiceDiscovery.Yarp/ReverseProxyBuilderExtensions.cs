using System;
using NetCorePal.ServiceDiscovery.Yarp;
using Yarp.ReverseProxy.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReverseProxyBuilderExtensions
    {
        /// <summary>
        /// Add ProxyConfigProvider witch load IProxyConfig from NetCorePalServiceDiscoveryClient
        /// </summary>
        /// <param name="builder"><seealso cref="IReverseProxyBuilder"/></param>
        /// <returns><seealso cref="IReverseProxyBuilder"/></returns>
        public static IReverseProxyBuilder LoadFromNetCorePalServiceDiscoveryClient(this IReverseProxyBuilder builder)
        {
            builder.Services.AddSingleton<IProxyConfigProvider, ServiceDiscoveryProxyConfigProvider>();
            return builder;
        }
    }
}

