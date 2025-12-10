using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public class DesignTimeShardingDatabaseDbContextFactory : IDesignTimeDbContextFactory<ShardingDatabaseDbContext>
{
    public ShardingDatabaseDbContext CreateDbContext(string[] args)
    {
        NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled = true;
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(DesignTimeShardingDatabaseDbContextFactory).Assembly));
        services.AddDbContext<ShardingDatabaseDbContext>(options =>
        {
            options.UseMySql(serverVersion: new MySqlServerVersion(new Version(8, 0, 32)),
                b => { b.MigrationsAssembly(typeof(DesignTimeShardingDatabaseDbContextFactory).Assembly.FullName); });
        });
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ShardingDatabaseDbContext>();
    }
}