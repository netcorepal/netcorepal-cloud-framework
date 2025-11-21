using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization; // 引入序列化命名空间
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // 引入依赖注入命名空间

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalMonitoringApi<TDbContext> : IMonitoringApi where TDbContext : DbContext, ICapDataStorage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISerializer _serializer;

    public NetCorePalMonitoringApi(IServiceProvider serviceProvider, ISerializer serializer)
    {
        _serviceProvider = serviceProvider;
        _serializer = serializer;
    }

    public async Task<MediumMessage?> GetPublishedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var message = await context.PublishedMessages.FindAsync(id);
        if (message == null) return null;

        return new NetCorePalMediumMessage
        {
            DbId = message.Id.ToString(),
            Origin = _serializer.Deserialize(message.Content ?? string.Empty)!,
            Content = message.Content ?? string.Empty,
            Added = message.Added,
            ExpiresAt = message.ExpiresAt,
            Retries = message.Retries ?? 0,
            DataSourceName = message.DataSourceName
        };
    }

    public async Task<MediumMessage?> GetReceivedMessageAsync(long id)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var message = await context.ReceivedMessages.FindAsync(id);
        if (message == null) return null;

        return new NetCorePalMediumMessage
        {
            DbId = message.Id.ToString(),
            Origin = _serializer.Deserialize(message.Content ?? string.Empty)!,
            Content = message.Content ?? string.Empty,
            Added = message.Added,
            ExpiresAt = message.ExpiresAt,
            Retries = message.Retries ?? 0
        };
    }

    public async Task<MediumMessage?> GetMessageAsync(long id, MessageType messageType)
    {
        return messageType switch
        {
            MessageType.Publish => await GetPublishedMessageAsync(id),
            MessageType.Subscribe => await GetReceivedMessageAsync(id),
            _ => throw new ArgumentException("Invalid message type", nameof(messageType))
        };
    }

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publishedSucceeded =
            await context.PublishedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Succeeded));
        var receivedSucceeded =
            await context.ReceivedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Succeeded));
        var publishedFailed =
            await context.PublishedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Failed));
        var receivedFailed = await context.ReceivedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Failed));

        return new StatisticsDto
        {
            PublishedSucceeded = publishedSucceeded,
            ReceivedSucceeded = receivedSucceeded,
            PublishedFailed = publishedFailed,
            ReceivedFailed = receivedFailed
        };
    }

    async Task<PagedQueryResult<MessageDto>> GetPublishedMessagesAsync(MessageQueryDto queryDto)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var query = context.PublishedMessages.AsNoTracking();

        if (!string.IsNullOrEmpty(queryDto.StatusName))
            query = query.Where(m => m.StatusName == StatusNameHelper.GetRealStatusName(queryDto.StatusName));

        if (!string.IsNullOrEmpty(queryDto.Name))
            query = query.Where(m => m.Name.ToLower() == queryDto.Name.ToLower());

        if (!string.IsNullOrEmpty(queryDto.Content))
            query = query.Where(m => m.Content != null && m.Content.ToLower().Contains(queryDto.Content.ToLower()));

        var total = await query.CountAsync();
        var items = await query.Skip(queryDto.CurrentPage * queryDto.PageSize).Take(queryDto.PageSize).ToListAsync();

        var result = items.Select(m => new MessageDto
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            Group = null,
            Content = m.Content,
            Retries = m.Retries ?? 0,
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            StatusName = m.StatusName
        }).ToList();

        return new PagedQueryResult<MessageDto>
        {
            Items = result,
            PageIndex = queryDto.CurrentPage,
            PageSize = queryDto.PageSize,
            Totals = total
        };
    }

    async Task<PagedQueryResult<MessageDto>> GetReceivedMessagesAsync(MessageQueryDto queryDto)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var query = context.ReceivedMessages.AsQueryable();

        if (!string.IsNullOrEmpty(queryDto.StatusName))
            query = query.Where(m => m.StatusName == StatusNameHelper.GetRealStatusName(queryDto.StatusName));

        if (!string.IsNullOrEmpty(queryDto.Name))
            query = query.Where(m => m.Name.ToLower() == queryDto.Name.ToLower());

        if (!string.IsNullOrEmpty(queryDto.Group))
            query = query.Where(m => m.Group != null && m.Group.ToLower() == queryDto.Group.ToLower());

        if (!string.IsNullOrEmpty(queryDto.Content))
            query = query.Where(m => m.Content != null && m.Content.ToLower().Contains(queryDto.Content.ToLower()));

        var total = await query.CountAsync();
        var items = await query.Skip(queryDto.CurrentPage * queryDto.PageSize).Take(queryDto.PageSize).ToListAsync();

        var result = items.Select(m => new MessageDto
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            Group = m.Group,
            Content = m.Content,
            Retries = m.Retries ?? 0,
            Added = m.Added,
            ExpiresAt = m.ExpiresAt,
            StatusName = m.StatusName
        }).ToList();

        return new PagedQueryResult<MessageDto>
        {
            Items = result,
            PageIndex = queryDto.CurrentPage,
            PageSize = queryDto.PageSize,
            Totals = total
        };
    }

    public async Task<PagedQueryResult<MessageDto>> GetMessagesAsync(MessageQueryDto queryDto)
    {
        return queryDto.MessageType == MessageType.Publish
            ? await GetPublishedMessagesAsync(queryDto)
            : await GetReceivedMessagesAsync(queryDto);
    }

    public async ValueTask<int> PublishedFailedCount()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.PublishedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Failed));
    }

    public async ValueTask<int> PublishedSucceededCount()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.PublishedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Succeeded));
    }

    public async ValueTask<int> ReceivedFailedCount()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.ReceivedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Failed));
    }

    public async ValueTask<int> ReceivedSucceededCount()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.ReceivedMessages.CountAsync(m => m.StatusName == nameof(StatusName.Succeeded));
    }

    public async Task<IDictionary<DateTime, int>> HourlySucceededJobs(MessageType type)
    {
        return await GetHourlyTimelineStats(type, nameof(StatusName.Succeeded));
    }

    public async Task<IDictionary<DateTime, int>> HourlyFailedJobs(MessageType type)
    {
        return await GetHourlyTimelineStats(type, nameof(StatusName.Failed));
    }

    private async Task<IDictionary<DateTime, int>> GetPublishedHourlyTimelineStats(string statusName)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var query = context.PublishedMessages.AsQueryable();

        var endDate = DateTime.Now;
        var startDate = endDate.AddHours(-24);

        var stats = await query
            .Where(m => m.StatusName == statusName && m.Added >= startDate && m.Added <= endDate)
            .GroupBy(m => new { m.Added.Year, m.Added.Month, m.Added.Day, m.Added.Hour })
            .Select(g => new
                { Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0), Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<DateTime, int>();
        for (var i = 0; i < 24; i++)
        {
            var time = new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 0, 0).AddHours(-i);
            result.Add(time, 0);
        }

        foreach (var stat in stats)
        {
            if (result.ContainsKey(stat.Time))
            {
                result[stat.Time] = stat.Count;
            }
        }

        return result;
    }

    private async Task<IDictionary<DateTime, int>> GetReceivedHourlyTimelineStats(string statusName)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var query = context.ReceivedMessages.AsQueryable();

        var endDate = DateTime.Now;
        var startDate = endDate.AddHours(-24);

        var stats = await query
            .Where(m => m.StatusName == statusName && m.Added >= startDate && m.Added <= endDate)
            .GroupBy(m => new { m.Added.Year, m.Added.Month, m.Added.Day, m.Added.Hour })
            .Select(g => new
                { Time = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0), Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<DateTime, int>();
        for (var i = 0; i < 24; i++)
        {
            var time = new DateTime(endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 0, 0).AddHours(-i);
            result.Add(time, 0);
        }

        foreach (var stat in stats)
        {
            if (result.ContainsKey(stat.Time))
            {
                result[stat.Time] = stat.Count;
            }
        }

        return result;
    }

    private async Task<IDictionary<DateTime, int>> GetHourlyTimelineStats(MessageType type, string statusName)
    {
        return type == MessageType.Publish
            ? await GetPublishedHourlyTimelineStats(statusName)
            : await GetReceivedHourlyTimelineStats(statusName);
    }
}