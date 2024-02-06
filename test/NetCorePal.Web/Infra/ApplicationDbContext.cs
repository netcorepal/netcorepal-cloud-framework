using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity;
using NetCorePal.Web.Infra.EntityConfigurations;

namespace NetCorePal.Web.Infra
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ApplicationDbContext : AppDbContextBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="mediator"></param>
        /// <param name="provider"></param>
        public ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(
            options, mediator, provider)
        {
            base.Database.EnsureCreated();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeliverRecordConfiguration());
            modelBuilder.ApplyConfiguration(new SagaEntityConfiguration());
        }

        
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Order> Orders => Set<Order>();
    }
}