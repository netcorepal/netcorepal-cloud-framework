using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Sqlite.UnitTests;

public class SqliteNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly string _dbPath;

    public SqliteNetCorePalDataStorageTests()
    {
        // Create a unique database file for this test instance
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_cap_{Guid.NewGuid()}.db");
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        
        // Clean up the database file after tests complete
        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_Sqlite()
    {
        await base.Storage_Tests();
    }
}
