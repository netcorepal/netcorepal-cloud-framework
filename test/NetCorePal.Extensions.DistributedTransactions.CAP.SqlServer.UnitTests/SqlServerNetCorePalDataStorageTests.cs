using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MsSql;
using Testcontainers.MySql;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests;

public class SqlServerNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly MsSqlContainer _msSqlContainer =
        new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04").Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_msSqlContainer.GetConnectionString(),
            b => { });
    }

    public override async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _msSqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_SqlServer()
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