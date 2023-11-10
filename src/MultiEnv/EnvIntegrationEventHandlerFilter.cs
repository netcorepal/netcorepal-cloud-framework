using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Options;
using NetCorePal.Context;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv;

public class EnvIntegrationEventHandlerFilter : IIntegrationEventHandlerFilter
{
    readonly IContextAccessor _contextAccessor;
    readonly IServiceDiscoveryClient _serviceDiscoveryClient;
    readonly EnvOptions _options;

    public EnvIntegrationEventHandlerFilter(IContextAccessor contextAccessor, IServiceDiscoveryClient serviceDiscoveryClient,
        IOptions<EnvOptions> options)
    {
        _contextAccessor = contextAccessor;
        _serviceDiscoveryClient = serviceDiscoveryClient;
        _options = options.Value;
    }
    
    public Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
    {
       
        var env = string.Empty;

        if (context.Headers.TryGetValue(EnvContext.ContextKey, out var envHeader) &&
            !string.IsNullOrEmpty(envHeader))
        {
            env = envHeader;
            _contextAccessor.SetContext(new EnvContext(env));
        }

        if (!IsEnvMatch(env) && IsDefaultEnv() && EnvServiceExists(env))
        {
            return Task.CompletedTask;
        }
        return next(context);
    }

    bool IsDefaultEnv()
    {
        return string.IsNullOrEmpty(_options.ServiceEnv) || _options.ServiceEnv == "default";
    }


    bool IsEnvMatch(string env)
    {
        return _options.ServiceEnv.Equals(env, StringComparison.OrdinalIgnoreCase);
    }

    bool EnvServiceExists(string env)
    {
        return _serviceDiscoveryClient.GetServiceClusters().ContainsKey($"{_options.ServiceName}-{env}");
    }

#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public sealed class SkipMessageException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
    }

    
}