namespace NetCorePal.Context;

public class TenantContext
{
    public static string ContextKey { get; set; } = "x-tenant";

    public TenantContext(string tenantId)
    {
        TenantId = tenantId;
    }

    public string TenantId { get; private set; }
}