using Hangfire;
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