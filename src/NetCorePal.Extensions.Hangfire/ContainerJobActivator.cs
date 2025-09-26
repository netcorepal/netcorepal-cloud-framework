using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Hangfire;

public class ContainerJobActivator : JobActivator
{
#pragma warning disable S2933
    private IServiceProvider _container;

    /// <summary>
    /// 容器job激活
    /// </summary>
    public ContainerJobActivator(IServiceProvider container)
    {
        _container = container;
    }

    /// <summary>
    /// 开始范围
    /// </summary>
#pragma warning disable S1133
    [Obsolete("Please implement/use the BeginScope(JobActivatorContext) method instead. Will be removed in 2.0.0.")]
#pragma warning restore S1133
    public override JobActivatorScope BeginScope()
    {
        return new ServiceScopeJobActivatorScope(_container.CreateScope());
    }

    /// <summary>
    /// 激活job
    /// </summary>
    public override object ActivateJob(Type jobType)
    {
        return _container.GetRequiredService(jobType);
    }
}