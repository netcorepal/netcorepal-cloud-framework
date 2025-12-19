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

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

/// <summary>
/// MongoDB-specific implementation that works around ExecuteUpdate/ExecuteDelete limitations
/// </summary>
public sealed class MongoDBNetCorePalDataStorage<TDbContext> : IDataStorage where TDbContext : DbContext, ICapDataStorage
{
    private readonly NetCorePalDataStorage<TDbContext> _baseStorage;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISerializer _serializer;

    public MongoDBNetCorePalDataStorage(IServiceProvider serviceProvider,
        ISnowflakeId snowflakeId, ISerializer serializer,
        IOptions<CapOptions> capOptions)
    {
        _baseStorage = new NetCorePalDataStorage<TDbContext>(serviceProvider, snowflakeId, serializer, capOptions);
        _serviceProvider = serviceProvider;
        _serializer = serializer;
    }

    // Delegate methods that don't use ExecuteUpdate/ExecuteDelete to base implementation
    public Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        => _baseStorage.AcquireLockAsync(key, ttl, instance, token);

    public Task ReleaseLockAsync(string key, string instance, CancellationToken token = default)
        => _baseStorage.ReleaseLockAsync(key, instance, token);

    public Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        => _baseStorage.RenewLockAsync(key, ttl, instance, token);

    public Task<MediumMessage> StoreMessageAsync(string name, Message content, object? transaction = null)
        => _baseStorage.StoreMessageAsync(name, content, transaction);

    public Task StoreReceivedExceptionMessageAsync(string name, string group, string content)
        => _baseStorage.StoreReceivedExceptionMessageAsync(name, group, content);

    public Task<MediumMessage> StoreReceivedMessageAsync(string name, string group, Message content)
        => _baseStorage.StoreReceivedMessageAsync(name, group, content);

    public Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
        => _baseStorage.GetPublishedMessagesOfNeedRetry(lookbackSeconds);

    public Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry(TimeSpan lookbackSeconds)
        => _baseStorage.GetReceivedMessagesOfNeedRetry(lookbackSeconds);

    public Task ScheduleMessagesOfDelayedAsync(Func<object, IEnumerable<MediumMessage>, Task> scheduleTask, CancellationToken token = default)
        => _baseStorage.ScheduleMessagesOfDelayedAsync(scheduleTask, token);

    public IMonitoringApi GetMonitoringApi()
        => _baseStorage.GetMonitoringApi();

    /// <summary>
    /// Change the state of PublishedMessages using Attach and Update approach
    /// </summary>
    public async Task ChangePublishStateToDelayedAsync(string[] ids)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var idList = ids.Select(p => long.Parse(p!)).ToList();
        
        var messages = await context.PublishedMessages
            .Where(m => idList.Contains(m.Id))
            .ToListAsync();
        
        foreach (var message in messages)
        {
            message.StatusName = nameof(StatusName.Delayed);
        }
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Change the state of a PublishMessage using Attach and Update approach
    /// </summary>
    public async Task ChangePublishStateAsync(MediumMessage message, StatusName state, object? transaction = null)
    {
        var dataSourceName = ((NetCorePalMediumMessage)message).DataSourceName;
        
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

        var id = long.Parse(message.DbId);
        var query = context.PublishedMessages.Where(m => m.Id == id);
        
        if (!string.IsNullOrEmpty(dataSourceName))
        {
            query = query.Where(m => m.DataSourceName == dataSourceName);
        }
        
        var entity = await query.FirstOrDefaultAsync();

        if (entity != null)
        {
            entity.Content = _serializer.Serialize(message.Origin);
            entity.Retries = message.Retries;
            entity.ExpiresAt = message.ExpiresAt;
            entity.StatusName = state.ToString();
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Change the state of a ReceivedMessage using Attach and Update approach
    /// </summary>
    public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        
        var id = long.Parse(message.DbId);
        var entity = await context.ReceivedMessages
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            entity.Content = _serializer.Serialize(message.Origin);
            entity.Retries = message.Retries;
            entity.ExpiresAt = message.ExpiresAt;
            entity.StatusName = state.ToString();
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Delete expired messages using Attach and Delete approach
    /// </summary>
    public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000, CancellationToken token = default)
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
            var messagesToDelete = await context.PublishedMessages.AsNoTracking()
                .Where(m => m.ExpiresAt != null && m.ExpiresAt < timeout && statusNames.Contains(m.StatusName))
                .OrderBy(m => m.Id)
                .Take(batchCount)
                .ToListAsync(token);

            if (messagesToDelete.Count == 0)
            {
                return 0;
            }

            context.PublishedMessages.RemoveRange(messagesToDelete);
            await context.SaveChangesAsync(token);
            return messagesToDelete.Count;
        }
        else if (table == NetCorePalStorageOptions.ReceivedMessageTableName)
        {
            var messagesToDelete = await context.ReceivedMessages.AsNoTracking()
                .Where(m => m.ExpiresAt != null && m.ExpiresAt < timeout && statusNames.Contains(m.StatusName))
                .OrderBy(m => m.Id)
                .Take(batchCount)
                .ToListAsync(token);

            if (messagesToDelete.Count == 0)
            {
                return 0;
            }

            context.ReceivedMessages.RemoveRange(messagesToDelete);
            await context.SaveChangesAsync(token);
            return messagesToDelete.Count;
        }

        throw new ArgumentException($"Invalid table name: {table}", nameof(table));
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Delete a ReceivedMessage using Attach and Delete approach
    /// </summary>
    public async Task<int> DeleteReceivedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        
        var entity = await context.ReceivedMessages
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            context.ReceivedMessages.Remove(entity);
            await context.SaveChangesAsync();
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Delete a PublishedMessage using Attach and Delete approach
    /// </summary>
    public async Task<int> DeletePublishedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        
        var entity = await context.PublishedMessages
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            context.PublishedMessages.Remove(entity);
            await context.SaveChangesAsync();
            return 1;
        }
        return 0;
    }
#endif
}
