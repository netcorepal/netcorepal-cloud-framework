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
        return NetCorePalStorageOptions.PublishedMessageTableName;
    }

    public string GetReceivedTableName()
    {
        return NetCorePalStorageOptions.ReceivedMessageTableName;
    }

    public string GetLockTableName()
    {
        return NetCorePalStorageOptions.LockTableName;
    }
}