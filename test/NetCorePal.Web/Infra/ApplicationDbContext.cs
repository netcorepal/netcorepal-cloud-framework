using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Web.Infra
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ApplicationDbContext : AppDbContextBase, 
        IShardingCore, IMySqlCapDataStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="mediator"></param>
        public ApplicationDbContext(DbContextOptions options, IMediator mediator) : base(
            options, mediator)
        {
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }


        /// <summary>
        /// 
        /// </summary>
        public DbSet<Order> Orders => Set<Order>();

        public DbSet<Int32Order> Int32Orders => Set<Int32Order>();

        public DbSet<Int64Order> Int64Orders => Set<Int64Order>();

        public DbSet<GuidOrder> GuidOrders => Set<GuidOrder>();
    }
}