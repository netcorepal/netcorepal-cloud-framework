namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalStorageOptions
{
    public static bool PublishedMessageShardingDatabaseEnabled { get; set; } = false;
    
    public static string PublishedMessageTableName { get; set; } = "CAPPublishedMessage";

    public static string ReceivedMessageTableName { get; set; } = "CAPReceivedMessage";

    public static string LockTableName { get; set; } = "CAPLock";
}