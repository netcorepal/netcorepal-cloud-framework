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

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public sealed class NetCorePalDataStorage<TDbContext> : IDataStorage where TDbContext : DbContext, ICapDataStorage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISnowflakeId _snowflakeId;
    private readonly ISerializer _serializer;
    private readonly PublishedMessageDataSourceContext _contextAccessor;
    private readonly IOptions<CapOptions> _capOptions;


    public NetCorePalDataStorage(IServiceProvider serviceProvider,
        ISnowflakeId snowflakeId, ISerializer serializer,
        IOptions<CapOptions> capOptions)
    {
        _serviceProvider = serviceProvider;
        _snowflakeId = snowflakeId;
        _contextAccessor = new PublishedMessageDataSourceContext();
        _serializer = serializer;
        _capOptions = capOptions;
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var lockEntry = await context.CapLocks.FindAsync([key], token);

        if (lockEntry == null)
        {
            context.CapLocks.Add(new CapLock
            {
                Key = key,
                Instance = instance,
                LastLockTime = DateTime.Now
            });
        }
        else if (lockEntry.LastLockTime == null || lockEntry.LastLockTime < DateTime.Now - ttl)
        {
            lockEntry.Instance = instance;
            lockEntry.LastLockTime = DateTime.Now;
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
        var lockEntry = await context.CapLocks.FindAsync([key], token);

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
            lockEntry.LastLockTime = lockEntry.LastLockTime?.Add(ttl) ?? DateTime.Now.Add(ttl);
            await context.SaveChangesAsync(token);
        }
    }

    public async Task ChangePublishStateToDelayedAsync(string[] ids)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var idList = ids.Select(p => long.Parse(p!)).ToList();
        await context.PublishedMessages
            .Where(m => idList.Contains(m.Id))
            .ExecuteUpdateAsync(m => m.SetProperty(p => p.StatusName, nameof(StatusName.Delayed)));
    }

    /// <summary>
    /// Change the state of a PublishMessage.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="state"></param>
    /// <param name="transaction">用以保存的事务</param>
    public async Task ChangePublishStateAsync(MediumMessage message, StatusName state, object? transaction = null)
    {
        var dataSourceName = ((NetCorePalMediumMessage)message).DataSourceName;

        async Task DoUpdate(TDbContext context) => await context.PublishedMessages
            .Where(m => m.Id == long.Parse(message.DbId))
            .WhereIf(!string.IsNullOrEmpty(dataSourceName), m => m.DataSourceName == dataSourceName)
            .ExecuteUpdateAsync(m => m
                .SetProperty(p => p.Content, _serializer.Serialize(message.Origin))
                .SetProperty(p => p.Retries, message.Retries)
                .SetProperty(p => p.ExpiresAt, message.ExpiresAt)
                .SetProperty(p => p.StatusName, state.ToString()));


        if (CapTransactionUnitOfWork.CurrentDbContext is TDbContext currentDbContext)
        {
            await DoUpdate(currentDbContext);
        }
        else
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            if (transaction != null)
            {
                var dbTrans = transaction as DbTransaction;
                if (dbTrans == null && transaction is IDbContextTransaction dbContextTrans)
                    dbTrans = dbContextTrans.GetDbTransaction();

                if (dbTrans != null)
                {
                    context.Database.SetDbConnection(dbTrans.Connection!);
                    await context.Database.UseTransactionAsync(dbTrans);
                }
            }
            await DoUpdate(context);
        }
    }

    /// <summary>
    /// Change the state of a ReceivedMessage.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="state"></param>
    public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.ReceivedMessages
            .Where(m => m.Id == long.Parse(message.DbId))
            .ExecuteUpdateAsync(m => m
                .SetProperty(p => p.Content, _serializer.Serialize(message.Origin))
                .SetProperty(p => p.Retries, message.Retries)
                .SetProperty(p => p.ExpiresAt, message.ExpiresAt)
                .SetProperty(p => p.StatusName, state.ToString()));
    }

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
            Added = DateTime.Now,
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

        if (CapTransactionUnitOfWork.CurrentDbContext is TDbContext currentDbContext)
        {
            currentDbContext.PublishedMessages.Add(message);
            await currentDbContext.SaveChangesAsync();
        }
        else
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            if (transaction != null)
            {
                if (CapTransactionUnitOfWork.CurrentDbContext != null)
                {
                    context = (TDbContext)CapTransactionUnitOfWork.CurrentDbContext!;
                }

                var dbTrans = transaction as DbTransaction;
                if (dbTrans == null && transaction is IDbContextTransaction dbContextTrans)
                    dbTrans = dbContextTrans.GetDbTransaction();

                if (dbTrans != null)
                {
                    context.Database.SetDbConnection(dbTrans.Connection!);
                    await context.Database.UseTransactionAsync(dbTrans);
                }
            }

            context.PublishedMessages.Add(message);
            await context.SaveChangesAsync();
        }

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
            Id = DateTime.Now.Ticks,
            Name = name,
            Group = group,
            Content = content,
            Retries = 0,
            Added = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(7),
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
            Added = DateTime.Now,
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

    public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000,
        CancellationToken token = new CancellationToken())
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        List<string> statusNames = [nameof(StatusName.Succeeded), nameof(StatusName.Failed)];
        if (table == NetCorePalStorageOptions.PublishedMessageTableName)
        {
            return await context.PublishedMessages
                .Where(m => m.ExpiresAt! < timeout && statusNames.Contains(m.StatusName)).Take(batchCount)
                .ExecuteDeleteAsync(token);
        }
        else if (table == NetCorePalStorageOptions.ReceivedMessageTableName)
        {
            return await context.ReceivedMessages
                .Where(m => m.ExpiresAt! < timeout && statusNames.Contains(m.StatusName)).Take(batchCount)
                .ExecuteDeleteAsync(token);
        }

        throw new ArgumentException(string.Format(R.InvalidTableName, table), nameof(table));
    }

    public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var threshold = DateTime.Now.Subtract(lookbackSeconds);
        var messages = await context.PublishedMessages.AsNoTracking()
            .Where(m => m.Retries < _capOptions.Value.FailedRetryCount && m.Added < threshold &&
                        (m.StatusName == nameof(StatusName.Failed) ||
                         m.StatusName == nameof(StatusName.Scheduled)))
            .Take(200)
            .ToListAsync();

        return messages.Select(m => new NetCorePalMediumMessage
        {
            DbId = m.Id.ToString(),
            Origin = _serializer.Deserialize(m.Content ?? string.Empty)!, // 处理可能的null引用
            Content = m.Content ?? string.Empty, // 确保Content不为null
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            Retries = m.Retries ?? 0, // 显式转换Retries
            DataSourceName = m.DataSourceName
        });
    }

    public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var threshold = DateTime.Now.Subtract(lookbackSeconds);
        var messages = await context.ReceivedMessages.AsNoTracking()
            .Where(m => m.Retries < _capOptions.Value.FailedRetryCount && m.Added < threshold &&
                        (m.StatusName == nameof(StatusName.Failed) ||
                         m.StatusName == nameof(StatusName.Scheduled)))
            .OrderBy(m => m.Added)
            .Take(200)
            .ToListAsync();

        return messages.Select(m => new MediumMessage
        {
            DbId = m.Id.ToString(),
            Origin = _serializer.Deserialize(m.Content ?? string.Empty)!, // 处理可能的null引用
            Content = m.Content ?? string.Empty, // 确保Content不为null
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            Retries = m.Retries ?? 0 // 显式转换Retries
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
        var twoMinutesLater = DateTime.Now.AddMinutes(2);
        var oneMinutesAgo = DateTime.Now.AddMinutes(-1);
        var messages = await context.PublishedMessages
            .Where(m => (m.StatusName == nameof(StatusName.Delayed) && m.ExpiresAt < twoMinutesLater) ||
                        (m.StatusName == nameof(StatusName.Queued) && m.ExpiresAt < oneMinutesAgo))
            .OrderBy(m => m.ExpiresAt)
#if NET9_0_OR_GREATER
            .Take(_capOptions.Value.SchedulerBatchSize)
#else
            .Take(200)
#endif
            .ToListAsync(token);

        await scheduleTask(transaction, messages.Select(m => new NetCorePalMediumMessage
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

#if NET9_0_OR_GREATER
    public async Task<int> DeleteReceivedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.ReceivedMessages
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<int> DeletePublishedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.PublishedMessages
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();
    }
#endif

    public IMonitoringApi GetMonitoringApi()
    {
        // Implement logic to return monitoring API
        return new NetCorePalMonitoringApi<TDbContext>(_serviceProvider, _serializer);
    }
}
