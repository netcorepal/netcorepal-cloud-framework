using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
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
    readonly ILogger _logger;

    public EnvIntegrationEventHandlerFilter(IContextAccessor contextAccessor,
        IServiceDiscoveryClient serviceDiscoveryClient,
        IOptions<EnvOptions> options, 
        ILogger<EnvIntegrationEventHandlerFilter> logger)
    {
        _contextAccessor = contextAccessor;
        _serviceDiscoveryClient = serviceDiscoveryClient;
        _options = options.Value;
        _logger = logger;
        if (string.IsNullOrEmpty(options.Value.ServiceName))
        {
            throw new ArgumentException("EnvOptions.ServiceName is required");
        }
    }

    public Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
    {
        var env = string.Empty;

        var envContext = _contextAccessor.GetContext<EnvContext>();
        if (envContext != null)
        {
            env = envContext.Env;
        }

        if (IsEnvMatch(env))
        {
            _logger.LogDebug("env matched, service={service}, service env={serviceEnv}, message env={messageEnv}",
                _options.ServiceName, _options.ServiceEnv, env);
            return next(context);
        }

        if (EnvServiceExists(env))
        {
            _logger.LogDebug(
                "skip event handler, env service exists, service={service}, service env={serviceEnv}, message env={messageEnv}",
                _options.ServiceName, _options.ServiceEnv, env);
            return Task.CompletedTask;
        }

        if (IsDefaultEnv())
        {
            _logger.LogDebug(
                "use default service, service={service}, service env={serviceEnv}, message env={messageEnv}",
                _options.ServiceName, _options.ServiceEnv, env);
            return next(context);
        }

        _logger.LogDebug(
            "skip event handler,env not match and is not default service, service={service}, service env={serviceEnv}, message env={messageEnv}",
            _options.ServiceName, _options.ServiceEnv, env);
        return Task.CompletedTask;
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