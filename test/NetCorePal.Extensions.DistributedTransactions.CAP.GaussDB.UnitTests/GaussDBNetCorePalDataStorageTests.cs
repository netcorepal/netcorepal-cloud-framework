using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.OpenGauss;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.GaussDB.UnitTests;

public class GaussDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly OpenGaussContainer _gaussDBContainer = new OpenGaussBuilder()
        .WithUsername("gaussdb") // 必须使用gaussdb用户才有权限创建数据库和表
        .WithPassword("Test@123456")
        .Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        // 默认数据库中EnsureCreatedAsync无效，会判定数据库存在，需要使用MigrateAsync
        optionsBuilder.UseGaussDB(_gaussDBContainer.GetConnectionString().Replace("postgres","postgres"));
    }

    public override async Task InitializeAsync()
    {
        await _gaussDBContainer.StartAsync();
        await base.InitializeAsync();
        
        
    }

    public override async Task DisposeAsync()
    {
        await _gaussDBContainer.StopAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_GaussDB()
    {
        await base.Storage_Tests();
    }
}
