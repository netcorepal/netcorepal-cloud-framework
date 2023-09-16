namespace NetCorePal.Context;

public class TenantContextCarrierHandler : IContextCarrierHandler
{
    public Type ContextType => typeof(TenantContext);

    public void Inject(IContextCarrier carrier, object? context)
    {
        if (context != null)
        {
            carrier.Set(TenantContext.ContextKey, ((TenantContext)context).TenantId.ToString());
        }
    }

    public object? Initial()
    {
        return null;
    }
}