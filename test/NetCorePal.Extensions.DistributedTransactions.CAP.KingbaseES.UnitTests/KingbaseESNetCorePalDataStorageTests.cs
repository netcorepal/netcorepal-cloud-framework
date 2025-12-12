using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.KingbaseES.UnitTests;

public class KingbaseESNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly IContainer _kingbaseESContainer;

    public KingbaseESNetCorePalDataStorageTests()
    {
        // Create KingbaseES container
        _kingbaseESContainer = new ContainerBuilder()
            .WithImage("apecloud/kingbase:v008r006c009b0014-unit")
            .WithPortBinding(54321, true)
            .WithEnvironment("ENABLE_CI", "yes")
            .WithEnvironment("DB_USER", "system")
            .WithEnvironment("DB_PASSWORD", "Test@123")
            .WithEnvironment("DB_MODE", "oracle")
            .WithPrivileged(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(54321))
            .Build();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        var port = _kingbaseESContainer.GetMappedPublicPort(54321);
        // KingbaseES default username is 'system', database is 'TEST'
        var connectionString = $"Host=localhost;Port={port};Database=TEST;Username=system;Password=Test@123;Timeout=30;";
        optionsBuilder.UseKdbndp(connectionString);
    }

    public override async Task InitializeAsync()
    {
        await _kingbaseESContainer.StartAsync();
        // Wait for KingbaseES to be fully initialized
        await WaitForDatabaseReadyAsync();
        await base.InitializeAsync();
    }

    private async Task WaitForDatabaseReadyAsync()
    {
        var maxRetries = 30;
        var delay = TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var port = _kingbaseESContainer.GetMappedPublicPort(54321);
                var connectionString = $"Host=localhost;Port={port};Database=TEST;Username=system;Password=Test@123;Timeout=5;";
                var optionsBuilder = new DbContextOptionsBuilder<NetCorePalDataStorageDbContext>();
                optionsBuilder.UseKdbndp(connectionString);
                
                await using var context = new NetCorePalDataStorageDbContext(optionsBuilder.Options, null!);
                await context.Database.CanConnectAsync();
                return;
            }
            catch
            {
                if (i == maxRetries - 1) throw;
                await Task.Delay(delay);
            }
        }
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
