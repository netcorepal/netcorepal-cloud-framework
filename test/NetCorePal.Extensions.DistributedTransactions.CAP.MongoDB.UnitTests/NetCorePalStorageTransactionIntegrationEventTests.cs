using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB.UnitTests;

[Collection("TransactionIntegration")]
public class NetCorePalStorageTransactionIntegrationEventTests(NetCorePalStorageMockHost host) : IClassFixture<NetCorePalStorageMockHost>
{
    [Fact]
    public async Task IntegrationEventHandler_Should_Not_Invoke_Before_Transaction_Commit()
    {
        MockEntityCreatedIntegrationEventHandler.LastTime = DateTimeOffset.MinValue;
        using var scope = host.HostInstance!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        var mockEntity = new MockEntity("test");
        ITransactionUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
        Assert.IsType<CapTransactionUnitOfWork>(unitOfWork);
        unitOfWork.CurrentTransaction = await unitOfWork.BeginTransactionAsync();
        Assert.NotNull(unitOfWork.CurrentTransaction);
        dbContext.MockEntities.Add(mockEntity);
        await unitOfWork.SaveEntitiesAsync();
        await Task.Delay(2000);
        var now = DateTimeOffset.UtcNow;
        Assert.Equal(DateTimeOffset.MinValue, MockEntityCreatedIntegrationEventHandler.LastTime);
        await unitOfWork.CommitAsync();
        await Task.Delay(2000);
        Assert.True(MockEntityCreatedIntegrationEventHandler.LastTime > now);
        await Task.Delay(2000);
    }
}
