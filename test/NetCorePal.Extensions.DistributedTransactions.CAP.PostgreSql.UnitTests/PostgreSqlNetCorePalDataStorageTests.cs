using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.PostgreSql;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql.UnitTests;

public class PostgreSqlNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_postgreSqlContainer.GetConnectionString(),
            b => {  });
    }

    public override async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_Npgsql()
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