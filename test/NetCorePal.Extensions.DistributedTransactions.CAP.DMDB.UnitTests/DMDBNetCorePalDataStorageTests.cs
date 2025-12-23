using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.DMDB;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.DMDB.UnitTests;

public class DMDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly DmdbContainer _dmDbContainer = new DmdbBuilder()
        .WithUsername("mydb")
        .WithPassword("MyPassword123")
        .WithDbaPassword("SYSDBA_abc123")
        .Build();
    
    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDm(_dmDbContainer.GetConnectionString());
    }

    public override async Task InitializeAsync()
    {
        await _dmDbContainer.StartAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _dmDbContainer.StopAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_DMDB()
    {
        await base.Storage_Tests();
    }
}
