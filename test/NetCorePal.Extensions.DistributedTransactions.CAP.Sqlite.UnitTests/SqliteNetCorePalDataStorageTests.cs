using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Sqlite.UnitTests;

public class SqliteNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly string _connectionString = "Data Source=:memory:";

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_Sqlite()
    {
        await base.Storage_Tests();
    }
}
