using Microsoft.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public interface ICapDataStorage
{
    /// <summary>
    /// Published messages.
    /// </summary>
    public DbSet<PublishedMessage> PublishedMessages { get; }

    /// <summary>
    /// Received messages. 
    ///</summary>
    public DbSet<ReceivedMessage> ReceivedMessages { get; }
    
    /// <summary>
    /// CAP Database Storage Locks.
    /// </summary>
    public DbSet<CapLock> CapLocks { get; }
}