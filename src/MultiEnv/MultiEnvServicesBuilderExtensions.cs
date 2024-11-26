using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.MultiEnv;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.DependencyInjection;

public static class MultiEnvServicesBuilderExtensions
{
    public static IMultiEnvServicesBuilder AddEnvServiceSelector(this IMultiEnvServicesBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IServiceSelector, EnvServiceSelector>());
        return builder;
    }

    public static IMultiEnvServicesBuilder AddEnvIntegrationFilters(
        this IMultiEnvServicesBuilder builder)
    {
        builder.Services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
        return builder;
    }
}

