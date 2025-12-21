using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.DMDB.UnitTests;

public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<NetCorePalDataStorageDbContext>
{
    public NetCorePalDataStorageDbContext CreateDbContext(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(DesignTimeApplicationDbContextFactory).Assembly));
        services.AddDbContext<NetCorePalDataStorageDbContext>(options =>
        {
            options.UseDm("Server=any;User ID=any;Password=any;Database=any");
        });
        var provider = services.BuildServiceProvider();
        var dbContext = provider.CreateScope().ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        return dbContext;
    }
}