using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.KingbaseES;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.KingbaseES.UnitTests;

public class KingbaseESNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly KingbaseESContainer _kingbaseESContainer = new KingbaseESBuilder()
        .WithDatabase("mytestdb")
        .WithUsername("myuser")
        .WithPassword("Test@123456")
        .Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        // 默认数据库中EnsureCreatedAsync无效，会判定数据库存在，需要使用MigrateAsync
        optionsBuilder.UseKdbndp(_kingbaseESContainer.GetConnectionString());
    }

    public override async Task InitializeAsync()
    {
        await _kingbaseESContainer.StartAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _kingbaseESContainer.StopAsync();
        await base.DisposeAsync();
    }

    //[Fact]
    public async Task Test_NetCorePalDataStorage_Use_KingbaseES()
    {
        await base.Storage_Tests();
    }
}
