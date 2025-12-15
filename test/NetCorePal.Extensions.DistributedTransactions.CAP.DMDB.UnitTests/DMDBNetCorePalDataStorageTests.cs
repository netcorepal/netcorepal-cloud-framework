using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.DMDB.UnitTests;

public class DMDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private const int dbPort = 5236;

    /// <summary>
    /// 默认的账号密码 SYSDBA/SYSDBA
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    private string GetConnectionString(int port)
    {
        // dmdb 的连接字符串格式：密码有格式要求，不能带特殊字符。
        return $"Host=localhost;Port={port};Username=testdb;Password=TestDm123;DBAPassword=SYSDBA_abc123;Timeout=30;";
    }
    
    private readonly IContainer _dmDbContainer;

    public DMDBNetCorePalDataStorageTests()
    {
        // 达梦没有官方的Docker镜像
        _dmDbContainer = new ContainerBuilder()
            .WithImage("cnxc/dm8:20250423-kylin")
            .WithPortBinding(dbPort, true)
            .WithPrivileged(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(dbPort))
            .Build();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        var port = _dmDbContainer.GetMappedPublicPort(dbPort);
        var connectionString = GetConnectionString(port);
        optionsBuilder.UseDm(connectionString);
    }

    public override async Task InitializeAsync()
    {
        await _dmDbContainer.StartAsync();
        
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
                var port = _dmDbContainer.GetMappedPublicPort(dbPort);
                var optionsBuilder = new DbContextOptionsBuilder<NetCorePalDataStorageDbContext>();
                var connectionString = GetConnectionString(port);
                optionsBuilder.UseDm(connectionString);
                
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
        await _dmDbContainer.StopAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_DMDB()
    {
        await base.Storage_Tests();
    }
}
