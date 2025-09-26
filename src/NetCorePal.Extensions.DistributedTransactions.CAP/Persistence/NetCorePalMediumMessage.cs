using DotNetCore.CAP.Persistence;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalMediumMessage : MediumMessage
{
    public string DataSourceName { get; set; } = string.Empty;
}