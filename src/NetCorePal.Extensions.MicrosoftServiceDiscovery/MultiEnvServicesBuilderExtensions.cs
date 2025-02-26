using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.MicrosoftServiceDiscovery;
using NetCorePal.Extensions.MultiEnv;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.DependencyInjection;

public static class MultiEnvServicesBuilderExtensions
{
    /// <summary>
    /// 使用NetCorePalServiceDiscovery作为多环境服务发现
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IMultiEnvServicesBuilder UseMicrosoftServiceDiscovery(this IMultiEnvServicesBuilder builder)
    {
        builder.Services.AddSingleton<IServiceChecker, MicrosoftServiceDiscoveryServiceChecker>();
        
        builder.Services.AddSingleton<MicrosoftServiceDiscoveryServiceChecker>();
        builder.Services.AddSingleton<ServiceEndpointWatcherFactory>();
        builder.Services.Configure<MultiEnvMicrosoftServiceDiscoveryOptions>(options =>
        {
            options.EnvServiceNameFunc = (hostName, env) => $"{hostName}-{env}";
        });
        builder.Services.AddServiceDiscovery();
        return builder;
    }
}