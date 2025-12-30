using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB;

/// <summary>
/// MongoDB-specific CAP options extension
/// </summary>
public class MongoDBNetCorePalCapOptionsExtension<TDbContext> : ICapOptionsExtension
    where TDbContext : DbContext, IMongoDBCapDataStorage
{
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapStorageMarkerService("NetCorePal.MongoDB"));
        services.AddSingleton<IDataStorage, MongoDBNetCorePalDataStorage<TDbContext>>();
        services.TryAddSingleton<IStorageInitializer, NetCorePalStorageInitializer>();
        if (services.Any(p => p.ServiceType == typeof(ICapTransactionFactory)))
        {
            throw new InvalidOperationException("A CAP transaction factory has already been registered. You cannot use both UseNetCorePalStorage and UseMongoDBNetCorePalStorage. Choose one based on your database provider: UseNetCorePalStorage for SQL databases or UseMongoDBNetCorePalStorage for MongoDB.");
        }

        services.TryAddScoped<ICapTransactionFactory, NetCorePalCapTransactionFactory>();
    }
}

/// <summary>
/// Extension methods for configuring MongoDB-specific CAP storage
/// </summary>
public static class MongoDBCapOptionsExtensions
{
    /// <summary>
    /// Use MongoDB-specific NetCorePal storage implementation that works around ExecuteUpdate/ExecuteDelete limitations
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type implementing IMongoDBCapDataStorage</typeparam>
    /// <param name="options">CAP options</param>
    /// <returns>CAP options</returns>
    public static CapOptions UseMongoDBNetCorePalStorage<TDbContext>(this CapOptions options)
        where TDbContext : DbContext, IMongoDBCapDataStorage
    {
        options.RegisterExtension(new MongoDBNetCorePalCapOptionsExtension<TDbContext>());
        return options;
    }
}
