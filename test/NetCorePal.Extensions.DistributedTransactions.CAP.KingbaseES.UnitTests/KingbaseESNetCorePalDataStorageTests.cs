using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.KingbaseES;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.KingbaseES.UnitTests;

public class KingbaseESNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly KingbaseESContainer _kingbaseESContainer = new KingbaseESBuilder()
        .WithDatabase("TEST")
        .WithUsername("system")
        .WithPassword("Test@123")
        .Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
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

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_KingbaseES()
    {
        await base.Storage_Tests();
    }
}
