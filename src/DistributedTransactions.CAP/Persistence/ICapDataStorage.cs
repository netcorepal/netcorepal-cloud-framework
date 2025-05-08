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

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}