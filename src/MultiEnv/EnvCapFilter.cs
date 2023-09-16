using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Options;
using NetCorePal.Context;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv;

public class EnvCapFilter : SubscribeFilter
{
    readonly IContextAccessor _contextAccessor;
    readonly IServiceDiscoveryClient _serviceDiscoveryClient;
    readonly EnvOptions _options;

    public EnvCapFilter(IContextAccessor contextAccessor, IServiceDiscoveryClient serviceDiscoveryClient,
        IOptions<EnvOptions> options)
    {
        _contextAccessor = contextAccessor;
        _serviceDiscoveryClient = serviceDiscoveryClient;
        _options = options.Value;
    }

    public override Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        var env = string.Empty;

        if (context.DeliverMessage.Headers.TryGetValue(EnvContext.ContextKey, out var envHeader) &&
            !string.IsNullOrEmpty(envHeader))
        {
            env = envHeader;
            _contextAccessor.SetContext(new EnvContext(env));
        }

        if (!IsEnvMatch(env) && IsDefaultEnv() && EnvServiceExists(env))
        {
            throw new SkipMessageException();
        }

        return Task.CompletedTask;
    }

    public override Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        // 订阅方法执行后
        return Task.CompletedTask;
    }

    public override Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is SkipMessageException)
        {
            context.ExceptionHandled = true;
        }

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

    class SkipMessageException : Exception
    {
    }
}