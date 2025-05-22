using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NetCorePal.Context;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalCapOptionsExtension<TDbContext> : ICapOptionsExtension
    where TDbContext : DbContext, ICapDataStorage
{
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapStorageMarkerService("NetCorePal"));
        services.AddSingleton<IDataStorage, NetCorePalDataStorage<TDbContext>>();
        services.TryAddSingleton<IStorageInitializer, NetCorePalStorageInitializer>();
        services.TryAddScoped<ICapTransactionFactory, NetCorePalCapTransactionFactory>();
    }
}