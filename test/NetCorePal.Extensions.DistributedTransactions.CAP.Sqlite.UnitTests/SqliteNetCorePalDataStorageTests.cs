using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Sqlite.UnitTests;

public class SqliteNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private SqliteConnection? _connection;

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        // Create and open a connection to an in-memory database
        // Keep the connection open so the database persists throughout the test
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        optionsBuilder.UseSqlite(_connection);
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        
        // Close and dispose the connection after tests complete
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_Sqlite()
    {
        await base.Storage_Tests();
    }
}
