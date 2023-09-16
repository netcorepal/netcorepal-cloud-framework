namespace NetCorePal.Context;

public class TenantContext
{
    public static string ContextKey = "x-tenant";

    public TenantContext(TenantId tenantId)
    {
        TenantId = tenantId;
    }

    public TenantId TenantId { get; private set; }
}

