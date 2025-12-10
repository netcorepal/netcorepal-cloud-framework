using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public class DesignTimeShardingTableDbContextFactory : IDesignTimeDbContextFactory<ShardingTableDbContext>
{
    public ShardingTableDbContext CreateDbContext(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(DesignTimeShardingTableDbContextFactory).Assembly));
        services.AddDbContext<ShardingTableDbContext>(options =>
        {
            options.UseMySql(serverVersion: new MySqlServerVersion(new Version(8, 0, 32)),
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeShardingTableDbContextFactory).Assembly.FullName);
                });
        });
        var provider = services.BuildServiceProvider();
        var dbContext = provider.CreateScope().ServiceProvider.GetRequiredService<ShardingTableDbContext>();
        return dbContext;
    }
}