using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MySql;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql.UnitTests;

public class MySqlNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_mySqlContainer.GetConnectionString(),
            new MySqlServerVersion(new Version(8, 0, 34)));
    }

    public override async Task InitializeAsync()
    {
        await _mySqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _mySqlContainer.StopAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_MySql()
    {
        await base.Storage_Tests();
    }

    [Fact]
    public async Task Test_StoreMessageAsync_WithTransaction()
    {
        await base.StoreMessageAsync_WithTransaction_ShouldUseProvidedTransaction();
    }

    [Fact]
    public async Task Test_ChangePublishStateAsync_WithTransaction()
    {
        await base.ChangePublishStateAsync_WithTransaction_ShouldUseProvidedTransaction();
    }

    [Fact]
    public async Task Test_StoreMessageAsync_WithoutTransaction()
    {
        await base.StoreMessageAsync_WithoutTransaction_ShouldWorkAsExpected();
    }

    [Fact]
    public async Task Test_ChangePublishStateAsync_WithoutTransaction()
    {
        await base.ChangePublishStateAsync_WithoutTransaction_ShouldWorkAsExpected();
    }
}