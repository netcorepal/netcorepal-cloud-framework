using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Sqlite.UnitTests;

public class SqliteNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly string _dbPath;

    public SqliteNetCorePalDataStorageTests()
    {
        // Create a unique database file for this test instance
        _dbPath = $"{Guid.NewGuid()}.db";
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");
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