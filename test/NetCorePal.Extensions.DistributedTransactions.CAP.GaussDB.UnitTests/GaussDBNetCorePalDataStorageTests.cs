using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.GaussDB;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.GaussDB.UnitTests;

public class GaussDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly GaussDBContainer _gaussDBContainer = new GaussDBBuilder()
        .WithDatabase("testdb")
        .WithUsername("gaussdb")
        .WithPassword("Test@123")
        .Build();


    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseGaussDB(_gaussDBContainer.GetConnectionString());
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
