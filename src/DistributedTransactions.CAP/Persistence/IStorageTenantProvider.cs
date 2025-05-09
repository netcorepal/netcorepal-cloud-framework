namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public interface IStorageTenantProvider
{
    public string GetTenantId();
}

public class DefaultStorageTenantProvider : IStorageTenantProvider
{
    public string GetTenantId()
    {
        return string.Empty;
    }
}