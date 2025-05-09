using DotNetCore.CAP;
using DotNetCore.CAP.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class NetCorePalCapOptionsExtension<TDbContext> : ICapOptionsExtension
    where TDbContext : DbContext, ICapDataStorage
{
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapStorageMarkerService("NetCorePal"));
        services.AddSingleton<IStorageLock, EmptyStorageLock>();
        services.Configure<NetCorePalStorageOptions>(p =>
        {
            NetCorePalStorageOptions.Default = new NetCorePalStorageOptions();
        });
        //TODO: 添加NetCorePalStorageOptions关于Tenant的配置
        NetCorePalStorageOptions.Default = new NetCorePalStorageOptions();
        services.AddSingleton<IDataStorage, NetCorePalDataStorage<TDbContext>>();
        services.TryAddSingleton<IStorageInitializer, NetCorePalStorageInitializer>();
        services.AddSingleton<IStorageTenantProvider, DefaultStorageTenantProvider>();
        services.TryAddScoped<ICapTransactionFactory, NetCorePalCapTransactionFactory>();
    }
}