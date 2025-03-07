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
    readonly IServiceChecker _serviceChecker;
    readonly EnvOptions _options;
    readonly ILogger _logger;

    public EnvIntegrationEventHandlerFilter(IContextAccessor contextAccessor,
        IServiceChecker serviceChecker,
        IOptions<EnvOptions> options,
        ILogger<EnvIntegrationEventHandlerFilter> logger)
    {
        _contextAccessor = contextAccessor;
        _serviceChecker = serviceChecker;
        _options = options.Value;
        _logger = logger;
        if (string.IsNullOrEmpty(options.Value.ServiceName))
        {
            throw new ArgumentException("EnvOptions.ServiceName is required");
        }
    }

    public async Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
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
            await next(context);
            return;
        }

        if (await EnvServiceExists(env))
        {
            _logger.LogDebug(
                "skip event handler, env service exists, service={service}, service env={serviceEnv}, message env={messageEnv}",
                _options.ServiceName, _options.ServiceEnv, env);
            return;
        }

        if (IsDefaultEnv())
        {
            _logger.LogDebug(
                "use default service, service={service}, service env={serviceEnv}, message env={messageEnv}",
                _options.ServiceName, _options.ServiceEnv, env);
            await next(context);
            return;
        }

        _logger.LogDebug(
            "skip event handler,env not match and is not default service, service={service}, service env={serviceEnv}, message env={messageEnv}",
            _options.ServiceName, _options.ServiceEnv, env);
    }

    bool IsDefaultEnv()
    {
        return string.IsNullOrEmpty(_options.ServiceEnv) || _options.ServiceEnv == "default";
    }


    bool IsEnvMatch(string env)
    {
        return _options.ServiceEnv.Equals(env, StringComparison.OrdinalIgnoreCase);
    }

    async ValueTask<bool> EnvServiceExists(string env)
    {
        return await _serviceChecker.ServiceExist(_serviceChecker.GetEnvServiceName(_options.ServiceName, env));
    }

#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public sealed class SkipMessageException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
    }
}