using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }


        /// <summary>
        /// 
        /// </summary>
        public DbSet<Order> Orders => Set<Order>();
    }
}