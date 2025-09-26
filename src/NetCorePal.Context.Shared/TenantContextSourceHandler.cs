namespace NetCorePal.Context;

public class TenantContextSourceHandler : IContextSourceHandler
{
    public Type ContextType => typeof(TenantContext);

    public object? Extract(IContextSource source)
    {
        var tenantStringValue = source.Get(TenantContext.ContextKey);
        return string.IsNullOrEmpty(tenantStringValue) ? null : new TenantContext(tenantStringValue);
    }
}