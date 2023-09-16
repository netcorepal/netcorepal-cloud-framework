namespace NetCorePal.Context;

public class TenantContextSourceHandler : IContextSourceHandler
{
    public Type ContextType => typeof(TenantContext);

    public object? Extract(IContextSource source)
    {
        var env = source.Get(EnvContext.ContextKey);
        return string.IsNullOrEmpty(env) ? null : new TenantContext(env);
    }
}