using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public abstract class NetCorePalDataStorageTestsBase<TDbContext> : IAsyncLifetime
    where TDbContext : DbContext, ICapDataStorage, IUnitOfWork, ITransactionUnitOfWork
{
    public NetCorePalDataStorageTestsBase()
    {
        NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled = false;
    }

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    IHost _host = null!;

    public async Task Storage_Tests()
    {
        Assert.Equal("Failed", nameof(StatusName.Failed));
        var storage = _host.Services.GetRequiredService<IDataStorage>();
        var storageInitializer = _host.Services.GetRequiredService<IStorageInitializer>();
        var monitoringApi = storage.GetMonitoringApi();

        //Test lock
        Assert.True(await storage.AcquireLockAsync("abc", TimeSpan.FromSeconds(2), "instance1"));
        Assert.False(await storage.AcquireLockAsync("abc", TimeSpan.FromSeconds(2), "instance2"));
        Assert.True(await storage.AcquireLockAsync("abc2", TimeSpan.FromSeconds(2), "instance3"));

        //Test Release
        await storage.ReleaseLockAsync("abc", "instance1");
        Assert.True(await storage.AcquireLockAsync("abc", TimeSpan.FromSeconds(2), "instance2"));


        //Test Renew
        Assert.True(await storage.AcquireLockAsync("renew", TimeSpan.FromSeconds(2), "instance1"));
        await storage.RenewLockAsync("renew", TimeSpan.FromSeconds(3), "instance1");
        await Task.Delay(2000);
        Assert.False(await storage.AcquireLockAsync("renew", TimeSpan.FromSeconds(2), "instance2"));

        var header = new Dictionary<string, string?>()
        {
            { "a", "b" }
        };

        //ChangePublishStateAsync
        var message1 = await storage.StoreMessageAsync("testmessage", new Message(header, "message context"), null);
        Assert.NotNull(message1);
        Assert.Equal("message context", message1.Origin.Value);
        Assert.Equal("b", message1.Origin.Headers["a"]);
        Assert.Equal(0, message1.Retries);
        var publishedMessage = await GetMessageAsync(message1.DbId);
        Assert.NotNull(publishedMessage);
        Assert.Equal(nameof(StatusName.Scheduled), publishedMessage.StatusName);


        await storage.ChangePublishStateAsync(message1, StatusName.Queued);
        publishedMessage = await GetMessageAsync(message1.DbId);
        Assert.NotNull(publishedMessage);
        Assert.Equal(nameof(StatusName.Queued), publishedMessage.StatusName);


        //ChangePublishStateToDelayedAsync
        var message2 = await storage.StoreMessageAsync("testmessage", new Message(header, "message context2"), null);
        Assert.NotNull(message2);
        Assert.Equal("message context2", message2.Origin.Value);
        Assert.Equal("b", message2.Origin.Headers["a"]);
        Assert.Equal(0, message2.Retries);

        var message2Db = await GetMessageAsync(message2.DbId);
        Assert.NotNull(message2Db);
        Assert.Equal(nameof(StatusName.Scheduled), message2Db.StatusName);
        Assert.Equal(message2.Retries, message2Db.Retries);
        Assert.Equal(message2.Content, message2Db.Content);
        Assert.Equal(message2.Added.ToString("yyyy-MM-dd HH:mm:ss"),
            message2Db.Added.ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.Equal(message2.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            message2Db.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss"));


        await storage.ChangePublishStateToDelayedAsync([message2.DbId]);
        var delayedMessage = await GetMessageAsync(message2.DbId);
        Assert.NotNull(delayedMessage);
        Assert.Equal(nameof(StatusName.Delayed), delayedMessage.StatusName);

        //StoreReceivedExceptionMessageAsync
        await storage.StoreReceivedExceptionMessageAsync("errorMessage", "testgroup", "message context3");
        await storage.StoreReceivedExceptionMessageAsync("errorMessage", "testgroup", "message context3");
        var errorMessages = await GetReceivedMessagesByName("errorMessage");
        Assert.Equal(2, errorMessages.Count);
        Assert.Equal("testgroup", errorMessages[0].Group);
        Assert.Equal("testgroup", errorMessages[1].Group);
        Assert.Equal("message context3", errorMessages[0].Content);
        Assert.Equal("message context3", errorMessages[1].Content);
        Assert.Equal("errorMessage", errorMessages[0].Name);
        Assert.Equal("errorMessage", errorMessages[1].Name);
        Assert.Equal(0, errorMessages[0].Retries);
        Assert.Equal(0, errorMessages[1].Retries);
        Assert.Equal(nameof(StatusName.Failed), errorMessages[0].StatusName);
        Assert.Equal(nameof(StatusName.Failed), errorMessages[1].StatusName);

        //StoreReceivedMessageAsync
        var receivedMessage = await storage.StoreReceivedMessageAsync("receivedMessage", "testgroup",
            new Message(header, "message context4"));
        Assert.NotNull(receivedMessage);
        Assert.Equal("message context4", receivedMessage.Origin.Value);
        Assert.Equal("b", receivedMessage.Origin.Headers["a"]);
        Assert.Equal(0, receivedMessage.Retries);
        var receivedMessageFromDb = await GetReceivedMessageAsync(receivedMessage.DbId);
        Assert.NotNull(receivedMessageFromDb);
        Assert.Equal(nameof(StatusName.Scheduled), receivedMessageFromDb.StatusName);
        Assert.Equal(receivedMessage.Retries, receivedMessageFromDb.Retries);
        Assert.Equal(receivedMessage.Content, receivedMessageFromDb.Content);
        Assert.Equal(receivedMessage.Added.ToString("yyyy-MM-dd HH:mm:ss"),
            receivedMessageFromDb.Added.ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.Equal(receivedMessage.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            receivedMessageFromDb.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss"));

        //DeleteExpiresAsync
        await AddExpiresPublishedMeeage();
        var publishRetryList = await storage.GetPublishedMessagesOfNeedRetry(TimeSpan.FromSeconds(50));
        Assert.Equal(2, publishRetryList.Count());
        var count = await storage.DeleteExpiresAsync(storageInitializer.GetPublishedTableName(),
            DateTime.Now.AddDays(-1), 1000);
        Assert.Equal(2, count);

        await Assert.ThrowsAsync<Exception>(() => GetMessageAsync("0"));
        await Assert.ThrowsAsync<Exception>(() => GetMessageAsync("5"));
        var message = await GetMessageAsync("2");
        Assert.NotNull(message);
        Assert.Equal(nameof(StatusName.Scheduled), message.StatusName);


        await AddExpiresReceivedMeeage();
        var receivedRetryList = await storage.GetReceivedMessagesOfNeedRetry(TimeSpan.FromSeconds(50));
        Assert.Equal(2, receivedRetryList.Count());

        var count1 =
            await storage.DeleteExpiresAsync(storageInitializer.GetReceivedTableName(), DateTime.Now.AddDays(-1),
                1000);
        Assert.Equal(2, count1);
        await Assert.ThrowsAsync<Exception>(() => GetReceivedMessageAsync("0"));
        await Assert.ThrowsAsync<Exception>(() => GetReceivedMessageAsync("5"));
        var receivedMessage1 = await GetReceivedMessageAsync("2");
        Assert.NotNull(receivedMessage1);
        Assert.Equal(nameof(StatusName.Scheduled), receivedMessage1.StatusName);

        //GetPublishedMessagesOfNeedRetry
        var publishRetryList2 = await storage.GetPublishedMessagesOfNeedRetry(TimeSpan.FromSeconds(50));
        Assert.Single(publishRetryList2);

        //GetReceivedMessagesOfNeedRetry
        var receivedRetryList2 = await storage.GetReceivedMessagesOfNeedRetry(TimeSpan.FromSeconds(50));
        Assert.Single(receivedRetryList2);

        //ScheduleMessagesOfDelayedAsync
        await ClearPublishMessageAsync();
        await AddScheduleMessageAsync();

        bool funcCalled = false;
        Func<object, IEnumerable<MediumMessage>, Task> func = (o, messages) =>
        {
            funcCalled = true;
            var list = messages.ToList();
            Assert.Equal(2, list.Count());
            Assert.Contains(list, p => p.DbId == "1");
            Assert.Contains(list, p => p.DbId == "3");
            return Task.CompletedTask;
        };

        await storage.ScheduleMessagesOfDelayedAsync(func);
        Assert.True(funcCalled);


        #region monitoringApi

        await ClearPublishMessageAsync();
        await ClearReceivedMessageAsync();
        //TODO: 添加数据并验证更多断言
        await AddMonitoringMessagesAsync();

        var publishMessage = await monitoringApi.GetMessagesAsync(new MessageQueryDto
        {
            MessageType = MessageType.Publish,
            Name = "test",
            Content = "abc",
            PageSize = 10,
            StatusName = "Failed",
        });
        Assert.NotNull(publishMessage);
        Assert.Equal(0, publishMessage.Totals);

        var pm = await monitoringApi.GetPublishedMessageAsync(100);
        Assert.Null(pm);

        var receivedMessages = await monitoringApi.GetMessagesAsync(new MessageQueryDto
        {
            MessageType = MessageType.Subscribe,
            Name = "test",
            Content = "abc",
            Group = "aaa",
            PageSize = 10,
            StatusName = "Failed",
        });
        Assert.NotNull(receivedMessages);
        Assert.Equal(0, receivedMessages.Totals);

        var rm = await monitoringApi.GetReceivedMessageAsync(100);
        Assert.Null(rm);


        Assert.Equal(0, await monitoringApi.PublishedFailedCount());
        Assert.Equal(0, await monitoringApi.PublishedSucceededCount());
        Assert.Equal(0, await monitoringApi.ReceivedFailedCount());
        Assert.Equal(0, await monitoringApi.ReceivedSucceededCount());

        var hourlyFailed = await monitoringApi.HourlyFailedJobs(MessageType.Publish);
        var hourlySucceeded = await monitoringApi.HourlySucceededJobs(MessageType.Publish);

        var statistics = await monitoringApi.GetStatisticsAsync();

        #endregion

#if NET9_0_OR_GREATER
        //DeletePublishedMessageAsync
        var deleteTestMessage1 = await storage.StoreMessageAsync("deleteTest", new Message(header, "delete test 1"), null);
        Assert.NotNull(deleteTestMessage1);
        var deleteTestMessageDb1 = await GetMessageAsync(deleteTestMessage1.DbId);
        Assert.NotNull(deleteTestMessageDb1);
        
        var deleteResult1 = await storage.DeletePublishedMessageAsync(long.Parse(deleteTestMessage1.DbId));
        Assert.Equal(1, deleteResult1);
        
        await Assert.ThrowsAsync<Exception>(() => GetMessageAsync(deleteTestMessage1.DbId));

        //DeleteReceivedMessageAsync
        var deleteTestReceivedMessage = await storage.StoreReceivedMessageAsync("deleteReceivedTest", "testgroup",
            new Message(header, "delete received test"));
        Assert.NotNull(deleteTestReceivedMessage);
        var deleteTestReceivedMessageDb = await GetReceivedMessageAsync(deleteTestReceivedMessage.DbId);
        Assert.NotNull(deleteTestReceivedMessageDb);
        
        var deleteResult2 = await storage.DeleteReceivedMessageAsync(long.Parse(deleteTestReceivedMessage.DbId));
        Assert.Equal(1, deleteResult2);
        
        await Assert.ThrowsAsync<Exception>(() => GetReceivedMessageAsync(deleteTestReceivedMessage.DbId));
#endif

        //Test StoreMessageAsync with transaction
        await ClearPublishMessageAsync();
        string txMessageId;
        await using (var scope1 = _host.Services.CreateAsyncScope())
        {
            var context1 = scope1.ServiceProvider.GetRequiredService<TDbContext>();
            var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
            
            await using var transaction1 = await unitOfWork1.BeginTransactionAsync();
            var txMessage = await storage.StoreMessageAsync("transactionTest", new Message(header, "transaction test message"), transaction1);
            Assert.NotNull(txMessage);
            Assert.Equal("transaction test message", txMessage.Origin.Value);
            txMessageId = txMessage.DbId;
            
            await unitOfWork1.CommitAsync();
        }
        
        // Query after transaction scope is disposed and committed
        var persistedTxMessage = await GetMessageAsync(txMessageId);
        Assert.NotNull(persistedTxMessage);
        Assert.Equal("transactionTest", persistedTxMessage.Name);
        Assert.Equal(nameof(StatusName.Scheduled), persistedTxMessage.StatusName);

        //Test ChangePublishStateAsync with transaction
        var stateChangeMsg = await storage.StoreMessageAsync("stateChangeTest", new Message(header, "state change test"), null);
        Assert.NotNull(stateChangeMsg);
        var initialStateMsg = await GetMessageAsync(stateChangeMsg.DbId);
        Assert.Equal(nameof(StatusName.Scheduled), initialStateMsg.StatusName);
        
        await using (var scope2 = _host.Services.CreateAsyncScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<TDbContext>();
            var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
            
            await using var transaction2 = await unitOfWork2.BeginTransactionAsync();
            stateChangeMsg.Retries = 1;
            await storage.ChangePublishStateAsync(stateChangeMsg, StatusName.Failed, transaction2);
            await unitOfWork2.CommitAsync();
        }
        
        // Query after transaction scope is disposed and committed
        var updatedStateMsg = await GetMessageAsync(stateChangeMsg.DbId);
        Assert.NotNull(updatedStateMsg);
        Assert.Equal(nameof(StatusName.Failed), updatedStateMsg.StatusName);
        Assert.Equal(1, updatedStateMsg.Retries);
    }

    async Task<PublishedMessage> GetMessageAsync(string id)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.PublishedMessages.FindAsync([long.Parse(id)]) ?? throw new Exception("Message not found");
    }

    async Task ClearPublishMessageAsync()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.PublishedMessages.ExecuteDeleteAsync();
    }

    async Task AddScheduleMessageAsync()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var header = new Dictionary<string, string?>()
        {
            { "a", "b" }
        };
        var message = new Message(header, "expires message");
        var content = serializer.Serialize(message);

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 1,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(10),
            Retries = 0,
            StatusName = nameof(StatusName.Delayed)
        });
        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 2,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(130),
            Retries = 0,
            StatusName = nameof(StatusName.Delayed)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 3,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(-70),
            Retries = 0,
            StatusName = nameof(StatusName.Queued)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 4,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(1),
            Retries = 0,
            StatusName = nameof(StatusName.Queued)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 5,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(-70),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 6,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(1),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 7,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(130),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 8,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(-70),
            Retries = 0,
            StatusName = nameof(StatusName.Scheduled)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 9,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(1),
            Retries = 0,
            StatusName = nameof(StatusName.Scheduled)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 10,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(130),
            Retries = 0,
            StatusName = nameof(StatusName.Scheduled)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 11,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(-70),
            Retries = 0,
            StatusName = nameof(StatusName.Succeeded)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 12,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(1),
            Retries = 0,
            StatusName = nameof(StatusName.Succeeded)
        });

        context.PublishedMessages.Add(new PublishedMessage()
        {
            Id = 13,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(130),
            Retries = 0,
            StatusName = nameof(StatusName.Succeeded)
        });
        await context.SaveChangesAsync();
    }

    async Task AddExpiresPublishedMeeage()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var header = new Dictionary<string, string?>()
        {
            { "a", "b" }
        };
        var message = new Message(header, "expires message");
        var content = serializer.Serialize(message);
        await context.PublishedMessages.AddAsync(new PublishedMessage()
        {
            Id = 1,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });
        await context.PublishedMessages.AddAsync(new PublishedMessage()
        {
            Id = 2,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Scheduled)
        });
        await context.PublishedMessages.AddAsync(new PublishedMessage()
        {
            Id = 3,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Delayed)
        });
        await context.PublishedMessages.AddAsync(new PublishedMessage()
        {
            Id = 4,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Queued)
        });
        await context.PublishedMessages.AddAsync(new PublishedMessage()
        {
            Id = 5,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Succeeded)
        });

        await context.SaveChangesAsync();
    }

    async Task AddExpiresReceivedMeeage()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var header = new Dictionary<string, string?>()
        {
            { "a", "b" }
        };
        var message = new Message(header, "expires message");
        var content = serializer.Serialize(message);
        await context.ReceivedMessages.AddAsync(new ReceivedMessage()
        {
            Id = 1,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });
        await context.ReceivedMessages.AddAsync(new ReceivedMessage()
        {
            Id = 2,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Scheduled)
        });
        await context.ReceivedMessages.AddAsync(new ReceivedMessage()
        {
            Id = 3,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Delayed)
        });
        await context.ReceivedMessages.AddAsync(new ReceivedMessage()
        {
            Id = 4,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Queued)
        });
        await context.ReceivedMessages.AddAsync(new ReceivedMessage()
        {
            Id = 5,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddDays(-2),
            Retries = 0,
            StatusName = nameof(StatusName.Succeeded)
        });

        await context.SaveChangesAsync();
    }

    async Task AddMonitoringMessagesAsync()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
        var header = new Dictionary<string, string?>()
        {
            { "a", "b" }
        };
        var message = new Message(header, "expires message");
        var content = serializer.Serialize(message);

        context.PublishedMessages.Add(new PublishedMessage
        {
            Id = 1,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddHours(-1),
            ExpiresAt = DateTime.Now.AddSeconds(10),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });
        context.PublishedMessages.Add(new PublishedMessage
        {
            Id = 2,
            Name = "test",
            Content = content,
            Added = DateTime.Now.AddDays(-3),
            ExpiresAt = DateTime.Now.AddSeconds(10),
            Retries = 0,
            StatusName = nameof(StatusName.Failed)
        });
    }


    async Task<ReceivedMessage> GetReceivedMessageAsync(string id)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.ReceivedMessages.FindAsync([long.Parse(id)]) ?? throw new Exception("Message not found");
    }


    async Task<List<ReceivedMessage>> GetReceivedMessagesByName(string name)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await context.ReceivedMessages.Where(p => p.Name == name).ToListAsync();
    }

    async Task ClearReceivedMessageAsync()
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.ReceivedMessages.ExecuteDeleteAsync();
    }


    public virtual async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(NetCorePalDataStorageTestsBase<>).Assembly)
                        .AddUnitOfWorkBehaviors());
                services.AddDbContext<TDbContext>(ConfigDbContext);
                services.AddCap(op =>
                {
                    op.UseNetCorePalStorage<TDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddIntegrationEvents(typeof(TDbContext), typeof(NetCorePalDataStorageTestsBase<>))
                    .UseCap<TDbContext>(capbuilder =>
                    {
                        capbuilder.RegisterServicesFromAssemblies(typeof(TDbContext),
                            typeof(NetCorePalDataStorageTestsBase<>));
                    });
            })
            .Build();
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await context.Database.EnsureCreatedAsync();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _host.StartAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        await Task.Delay(3000);
    }

    public virtual async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }


    protected abstract void ConfigDbContext(DbContextOptionsBuilder optionsBuilder);
}