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
            .WithImage("enmotech/opengauss:latest")
            .WithPortBinding(5432, true)
            .WithEnvironment("GS_PASSWORD", "Test@123")
            .WithEnvironment("GS_USERNAME", "gaussdb")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        var port = _gaussDBContainer.GetMappedPublicPort(5432);
        var connectionString = $"Host=localhost;Port={port};Database=postgres;Username=gaussdb;Password=Test@123;Timeout=30;";
        optionsBuilder.UseGaussDB(connectionString);
    }

    public override async Task InitializeAsync()
    {
        await _gaussDBContainer.StartAsync();
        await Task.Delay(5000); // Wait for GaussDB to be ready
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
