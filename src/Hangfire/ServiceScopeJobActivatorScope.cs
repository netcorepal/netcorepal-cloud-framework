using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Hangfire;

public class ServiceScopeJobActivatorScope(IServiceScope scope) : JobActivatorScope
{
    /// <summary>
    /// 处理范围
    /// </summary>
    public override void DisposeScope()
    {
        scope.Dispose();
    }

    /// <summary>
    /// 获取服务
    /// </summary>
    public override object Resolve(Type type)
    {
        return scope.ServiceProvider.GetRequiredService(type);
    }
}