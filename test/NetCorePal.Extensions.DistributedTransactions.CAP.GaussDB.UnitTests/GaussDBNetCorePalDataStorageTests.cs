using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.GaussDB.UnitTests;

public class GaussDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        // Use in-memory GaussDB for testing, similar to PostgreSQL
        optionsBuilder.UseGaussDB("Host=localhost;Database=testdb;Username=test;Password=test");
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_GaussDB()
    {
        await base.Storage_Tests();
    }
}
