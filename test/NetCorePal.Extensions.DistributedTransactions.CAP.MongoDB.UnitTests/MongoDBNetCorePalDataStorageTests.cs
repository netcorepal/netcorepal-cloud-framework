using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MongoDb;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB.UnitTests;

public class MongoDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .Build();

    public override async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        await base.InitializeAsync();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMongoDB(_mongoContainer.GetConnectionString(), "test_cap_db");
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _mongoContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_MongoDB()
    {
        await base.Storage_Tests();
    }
}
