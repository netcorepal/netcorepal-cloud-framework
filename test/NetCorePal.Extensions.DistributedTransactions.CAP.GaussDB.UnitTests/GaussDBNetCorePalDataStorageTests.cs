using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.GaussDB.UnitTests;

public class GaussDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly IContainer _gaussDBContainer;

    public GaussDBNetCorePalDataStorageTests()
    {
        // Create OpenGauss container (GaussDB compatible)
        _gaussDBContainer = new ContainerBuilder()
            .WithImage("opengauss/opengauss:latest")
            .WithPortBinding(5432, true)
            .WithEnvironment("GS_PASSWORD", "Test@123")
            .WithPrivileged(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        var port = _gaussDBContainer.GetMappedPublicPort(5432);
        // OpenGauss default username is 'gaussdb', database is 'postgres'
        var connectionString = $"Host=localhost;Port={port};Database=testdb;Username=gaussdb;Password=Test@123;Timeout=30;";
        optionsBuilder.UseGaussDB(connectionString);
    }

    public override async Task InitializeAsync()
    {
        await _gaussDBContainer.StartAsync();
        // Wait for OpenGauss to be fully initialized
        // OpenGauss takes longer to start than standard PostgreSQL
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
                var port = _gaussDBContainer.GetMappedPublicPort(5432);
                var connectionString = $"Host=localhost;Port={port};Database=postgres;Username=gaussdb;Password=Test@123;Timeout=5;";
                var optionsBuilder = new DbContextOptionsBuilder<NetCorePalDataStorageDbContext>();
                optionsBuilder.UseGaussDB(connectionString);
                
                using var context = new NetCorePalDataStorageDbContext(optionsBuilder.Options, null!);
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
        await _gaussDBContainer.StopAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_GaussDB()
    {
        await base.Storage_Tests();
    }
}
