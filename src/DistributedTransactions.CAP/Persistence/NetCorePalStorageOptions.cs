namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalStorageOptions
{

    internal static NetCorePalStorageOptions? Default = null;
    
    public bool EnableTenant { get; set; } = false;
}