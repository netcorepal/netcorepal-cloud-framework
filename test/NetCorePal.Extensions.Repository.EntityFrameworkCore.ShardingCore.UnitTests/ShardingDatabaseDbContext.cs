using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public partial class ShardingDatabaseDbContext(
    DbContextOptions<ShardingDatabaseDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), IShardingTable, IShardingDatabase
{
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShardingDatabaseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
}