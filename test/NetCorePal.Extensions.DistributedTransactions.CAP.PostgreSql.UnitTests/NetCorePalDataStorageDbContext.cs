using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public partial class NetCorePalDataStorageDbContext(
    DbContextOptions<NetCorePalDataStorageDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), IPostgreSqlCapDataStorage
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NetCorePalDataStorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}