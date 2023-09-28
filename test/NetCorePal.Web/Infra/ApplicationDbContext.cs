using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity;
using NetCorePal.Web.Infra.EntityConfigurations;

namespace NetCorePal.Web.Infra
{
    public partial class ApplicationDbContext : AppIdentityDbContextBase
    {
        public ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(
            options, mediator, provider)
        {
            base.Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeliverRecordConfiguration());
            modelBuilder.ApplyConfiguration(new SagaEntityConfiguration());
        }

        public DbSet<Order> Orders => Set<Order>();
    }
}