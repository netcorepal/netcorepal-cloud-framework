using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

/// <summary>
/// MongoDB-specific implementation that works around ExecuteUpdate/ExecuteDelete limitations
/// </summary>
public sealed class MongoDBNetCorePalDataStorage<TDbContext> : IDataStorage
    where TDbContext : DbContext, ICapDataStorage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISnowflakeId _snowflakeId;
    private readonly ISerializer _serializer;
    private readonly PublishedMessageDataSourceContext _contextAccessor;
    private readonly IOptions<CapOptions> _capOptions;

    public MongoDBNetCorePalDataStorage(IServiceProvider serviceProvider,
        ISnowflakeId snowflakeId, ISerializer serializer,
        IOptions<CapOptions> capOptions)
    {
        _serviceProvider = serviceProvider;
        _snowflakeId = snowflakeId;
        _serializer = serializer;
        _capOptions = capOptions;
        _contextAccessor = new PublishedMessageDataSourceContext();
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var lockEntry = await context.CapLocks.FindAsync(new object[] { key }, token);

        if (lockEntry == null)
        {
            context.CapLocks.Add(new CapLock
            {
                Key = key,
                Instance = instance,
                LastLockTime = DateTime.UtcNow
            });
        }
        else if (lockEntry.LastLockTime == null || lockEntry.LastLockTime < DateTime.UtcNow - ttl)
        {
            lockEntry.Instance = instance;
            lockEntry.LastLockTime = DateTime.UtcNow;
        }
        else
        {
            return false;
        }

        await context.SaveChangesAsync(token);
        return true;
    }

    public async Task ReleaseLockAsync(string key, string instance, CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var lockEntry = await context.CapLocks.FindAsync(new object[] { key }, token);

        if (lockEntry != null && lockEntry.Instance == instance)
        {
            lockEntry.Instance = null;
            lockEntry.LastLockTime = null;
            await context.SaveChangesAsync(token);
        }
    }

    public async Task RenewLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var lockEntry = await context.CapLocks.FindAsync(new object[] { key }, token);

        if (lockEntry != null && lockEntry.Instance == instance)
        {
            lockEntry.LastLockTime = lockEntry.LastLockTime?.Add(ttl) ?? DateTime.UtcNow.Add(ttl);
            await context.SaveChangesAsync(token);
        }
    }

    /// <summary>
    /// Change the state of PublishedMessages using Attach and Update approach without querying
    /// </summary>
    public async Task ChangePublishStateToDelayedAsync(string[] ids)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var idList = ids.Select(p => long.Parse(p!)).ToList();

        foreach (var id in idList)
        {
            var message = new PublishedMessage { Id = id };
            context.PublishedMessages.Attach(message);
            message.StatusName = nameof(StatusName.Delayed);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Change the state of a PublishMessage using Attach and Update approach without querying
    /// </summary>
    public async Task ChangePublishStateAsync(MediumMessage message, StatusName state, object? transaction = null)
    {
        var dataSourceName = ((NetCorePalMediumMessage)message).DataSourceName;

        await using var scope = _serviceProvider.CreateAsyncScope();
        TDbContext context;
        if (transaction != null)
        {
            var dbContext = (transaction as NetCorePalCapDbTransaction)?.DbContext as TDbContext;
            if (dbContext != null)
            {
                context = dbContext;
            }
            else
            {
                throw new InvalidOperationException("The transaction is not valid.");
            }
        }
        else
        {
            context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        }

        var id = long.Parse(message.DbId);
        var entity = new PublishedMessage
        {
            Id = id,
            Content = _serializer.Serialize(message.Origin),
            Retries = message.Retries,
            ExpiresAt = message.ExpiresAt,
            StatusName = state.ToString()
        };

        if (!string.IsNullOrEmpty(dataSourceName))
        {
            entity.DataSourceName = dataSourceName;
        }

        context.PublishedMessages.Attach(entity);
        context.Entry(entity).Property(p => p.Content).IsModified = true;
        context.Entry(entity).Property(p => p.Retries).IsModified = true;
        context.Entry(entity).Property(p => p.ExpiresAt).IsModified = true;
        context.Entry(entity).Property(p => p.StatusName).IsModified = true;

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Change the state of a ReceivedMessage using Attach and Update approach without querying
    /// </summary>
    public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var id = long.Parse(message.DbId);
        var entity = new ReceivedMessage
        {
            Id = id,
            Content = _serializer.Serialize(message.Origin),
            Retries = message.Retries,
            ExpiresAt = message.ExpiresAt,
            StatusName = state.ToString()
        };

        context.ReceivedMessages.Attach(entity);
        context.Entry(entity).Property(p => p.Content).IsModified = true;
        context.Entry(entity).Property(p => p.Retries).IsModified = true;
        context.Entry(entity).Property(p => p.ExpiresAt).IsModified = true;
        context.Entry(entity).Property(p => p.StatusName).IsModified = true;

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete expired messages using Attach and Delete approach without full entity loading
    /// </summary>
    public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000,
        CancellationToken token = default)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var statusNames = new[]
        {
            nameof(StatusName.Succeeded),
            nameof(StatusName.Failed)
        };

        if (table == NetCorePalStorageOptions.PublishedMessageTableName)
        {
            var idsToDelete = await context.PublishedMessages.AsNoTracking()
                .Where(m => m.ExpiresAt != null && m.ExpiresAt < timeout && statusNames.Contains(m.StatusName))
                .OrderBy(m => m.Id)
                .Take(batchCount)
                .Select(m => m.Id)
                .ToListAsync(token);

            if (idsToDelete.Count == 0)
            {
                return 0;
            }

            foreach (var id in idsToDelete)
            {
                var entity = new PublishedMessage { Id = id };
                context.PublishedMessages.Attach(entity);
                context.PublishedMessages.Remove(entity);
            }

            await context.SaveChangesAsync(token);
            return idsToDelete.Count;
        }
        else if (table == NetCorePalStorageOptions.ReceivedMessageTableName)
        {
            var idsToDelete = await context.ReceivedMessages.AsNoTracking()
                .Where(m => m.ExpiresAt != null && m.ExpiresAt < timeout && statusNames.Contains(m.StatusName))
                .OrderBy(m => m.Id)
                .Take(batchCount)
                .Select(m => m.Id)
                .ToListAsync(token);

            if (idsToDelete.Count == 0)
            {
                return 0;
            }

            foreach (var id in idsToDelete)
            {
                var entity = new ReceivedMessage { Id = id };
                context.ReceivedMessages.Attach(entity);
                context.ReceivedMessages.Remove(entity);
            }

            await context.SaveChangesAsync(token);
            return idsToDelete.Count;
        }

        throw new ArgumentException($"Invalid table name: {table}", nameof(table));
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Delete a ReceivedMessage using Attach and Delete approach without querying
    /// </summary>
    public async Task<int> DeleteReceivedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var entity = new ReceivedMessage { Id = id };
        context.ReceivedMessages.Attach(entity);
        context.ReceivedMessages.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete a PublishedMessage using Attach and Delete approach without querying
    /// </summary>
    public async Task<int> DeletePublishedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var entity = new PublishedMessage { Id = id };
        context.PublishedMessages.Attach(entity);
        context.PublishedMessages.Remove(entity);
        return await context.SaveChangesAsync();
    }
#endif

    /// <summary>
    /// 保存 PublishedMessage 到库中，如果transaction不为null，则使用传入的事务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public async Task<MediumMessage> StoreMessageAsync(string name, Message content, object? transaction = null)
    {
        var serializedContent = _serializer.Serialize(content);
        var message = new PublishedMessage
        {
            Id = _snowflakeId.NextId(),
            Name = name,
            Content = serializedContent,
            Retries = 0,
            Added = DateTime.UtcNow,
            ExpiresAt = null,
            StatusName = nameof(StatusName.Scheduled)
        };
        if (NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled)
        {
            var databaseName = _contextAccessor.GetDataSourceName();
            if (!string.IsNullOrEmpty(databaseName))
            {
                message.DataSourceName = databaseName;
            }
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        TDbContext context;
        if (transaction != null)
        {
            var dbContext = (transaction as NetCorePalCapDbTransaction)?.DbContext as TDbContext;
            if (dbContext != null)
            {
                context = dbContext;
            }
            else
            {
                throw new InvalidOperationException("The transaction is not valid.");
            }
        }
        else
        {
            context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        }

        context.PublishedMessages.Add(message);
        await context.SaveChangesAsync();


        return new NetCorePalMediumMessage
        {
            DbId = message.Id.ToString(),
            Origin = content,
            Content = serializedContent,
            Added = message.Added,
            ExpiresAt = message.ExpiresAt,
            Retries = 0,
            DataSourceName = message.DataSourceName
        };
    }

    public async Task StoreReceivedExceptionMessageAsync(string name, string group, string content)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var message = new ReceivedMessage
        {
            Id = DateTime.UtcNow.Ticks,
            Name = name,
            Group = group,
            Content = content,
            Retries = 0,
            Added = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            StatusName = nameof(StatusName.Failed)
        };

        context.ReceivedMessages.Add(message);
        await context.SaveChangesAsync();
    }

    // 修正Content的序列化逻辑，确保序列化后的内容存储在数据库中
    public async Task<MediumMessage> StoreReceivedMessageAsync(string name, string group, Message content)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var serializedContent = _serializer.Serialize(content);
        var message = new ReceivedMessage
        {
            Id = _snowflakeId.NextId(),
            Name = name,
            Group = group,
            Content = serializedContent,
            Retries = 0,
            Added = DateTime.UtcNow,
            ExpiresAt = null,
            StatusName = nameof(StatusName.Scheduled)
        };

        context.ReceivedMessages.Add(message);
        await context.SaveChangesAsync();

        return new MediumMessage
        {
            DbId = message.Id.ToString(),
            Origin = content,
            Content = serializedContent,
            Added = message.Added,
            ExpiresAt = message.ExpiresAt,
            Retries = 0
        };
    }

    public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var threshold = DateTime.UtcNow.Subtract(lookbackSeconds);
        var messages = await context.PublishedMessages.AsNoTracking()
            .Where(m => m.Retries < _capOptions.Value.FailedRetryCount && m.Added < threshold &&
                        (m.StatusName == nameof(StatusName.Failed) ||
                         m.StatusName == nameof(StatusName.Scheduled)))
            .OrderBy(m => m.Id)
            .Take(200)
            .ToListAsync();

        return messages.Select(m => new NetCorePalMediumMessage
        {
            DbId = m.Id.ToString(),
            Origin = _serializer.Deserialize(m.Content ?? string.Empty)!,
            Content = m.Content ?? string.Empty,
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            Retries = m.Retries ?? 0,
            DataSourceName = m.DataSourceName
        });
    }

    public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var threshold = DateTime.UtcNow.Subtract(lookbackSeconds);
        var messages = await context.ReceivedMessages.AsNoTracking()
            .Where(m => m.Retries < _capOptions.Value.FailedRetryCount && m.Added < threshold &&
                        (m.StatusName == nameof(StatusName.Failed) ||
                         m.StatusName == nameof(StatusName.Scheduled)))
            .OrderBy(m => m.Id)
            .Take(200)
            .ToListAsync();

        return messages.Select(m => new MediumMessage
        {
            DbId = m.Id.ToString(),
            Origin = _serializer.Deserialize(m.Content ?? string.Empty)!,
            Content = m.Content ?? string.Empty,
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            Retries = m.Retries ?? 0
        });
    }

    public async Task ScheduleMessagesOfDelayedAsync(Func<object, IEnumerable<MediumMessage>, Task> scheduleTask,
        CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
        await using var transaction = await unitOfWork.BeginTransactionAsync(token);
        unitOfWork.CurrentTransaction = transaction;
        var dbTransaction = ((NetCorePalCapEFDbTransaction)unitOfWork.CurrentTransaction).CapTransaction
            .DbTransaction!;
        var twoMinutesLater = DateTime.UtcNow.AddMinutes(2);
        var oneMinutesAgo = DateTime.UtcNow.AddMinutes(-1);
        var messages = await context.PublishedMessages
            .Where(m => (m.StatusName == nameof(StatusName.Delayed) && m.ExpiresAt < twoMinutesLater) ||
                        (m.StatusName == nameof(StatusName.Queued) && m.ExpiresAt < oneMinutesAgo))
            .OrderBy(m => m.Id)
#if NET9_0_OR_GREATER
            .Take(_capOptions.Value.SchedulerBatchSize)
#else
            .Take(200)
#endif
            .ToListAsync(token);

        await scheduleTask(dbTransaction, messages.Select(m => new NetCorePalMediumMessage
        {
            DbId = m.Id.ToString(),
            Content = m.Content ?? string.Empty,
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            Retries = m.Retries ?? 0,
            DataSourceName = m.DataSourceName
        }));

        foreach (var message in messages)
        {
            message.StatusName = nameof(StatusName.Queued);
        }

        await context.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }

    public IMonitoringApi GetMonitoringApi()
    {
        return new MongoDBNetCorePalMonitoringApi<TDbContext>(_serviceProvider, _serializer);
    }
}