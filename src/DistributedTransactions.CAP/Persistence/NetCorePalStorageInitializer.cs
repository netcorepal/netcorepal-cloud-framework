using DotNetCore.CAP.Persistence;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalStorageInitializer : IStorageInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public string GetPublishedTableName()
    {
        return "PublishedMessage";
    }

    public string GetReceivedTableName()
    {
        return "ReceivedMessage";
    }

    public string GetLockTableName()
    {
        return "CapLock";
    }
}