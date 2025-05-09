using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CapOptionsExtensions
{
    public static CapOptions UseNetCorePalStorage<TDbContext>(this CapOptions options)
        where TDbContext : DbContext, ICapDataStorage
    {
        options.RegisterExtension(new NetCorePalCapOptionsExtension<TDbContext>());
        return options;
    }
}