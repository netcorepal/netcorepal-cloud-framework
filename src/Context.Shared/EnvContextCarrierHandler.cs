namespace NetCorePal.Context;

public class EnvContextCarrierHandler : IContextCarrierHandler
{
    public Type ContextType => typeof(EnvContext);

    public void Inject(IContextCarrier carrier, object? context)
    {
        if (context != null)
        {
            carrier.Set(EnvContext.ContextKey, ((EnvContext)context).Evn);
        }
    }

    public object? Initial()
    {
        return null;
    }
}