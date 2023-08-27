using System;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.ServiceDiscovery;
using NetCorePal.ServiceDiscovery.Client;
namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetCorePalServiceDiscoveryClient(this IServiceCollection services)
        {
            services.AddSingleton<IServiceDiscoveryClient, DefaultServiceDiscoveryClient>();
            return services;
        }
    }
}

