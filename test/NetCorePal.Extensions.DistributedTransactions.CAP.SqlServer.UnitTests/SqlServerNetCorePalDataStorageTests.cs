using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MsSql;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests;

public class SqlServerNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly MsSqlContainer _msSqlContainer =
        new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04").Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_msSqlContainer.GetConnectionString().Replace("master", "test_cap_db"),
            b => { });
    }

    public override async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await base.InitializeAsync();
        // READ_COMMITTED_SNAPSHOT 开启避免 读取未提交的数据的测试用例超时问题，正常使用时可以不开启
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("ALTER DATABASE [test_cap_db] SET READ_COMMITTED_SNAPSHOT ON;");
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
}