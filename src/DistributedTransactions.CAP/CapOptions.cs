using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public class CapOptions
{
    public Action<PublisherDelegate> PublisherFilterConfigure { get; set; } = _ => { };
}