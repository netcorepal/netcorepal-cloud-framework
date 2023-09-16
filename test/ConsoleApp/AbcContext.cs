using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.ConsoleApp
{
    public partial class AbcContext : AppDbContextBase
    {
        public AbcContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options,
            mediator, provider)
        {
        }


        public DbSet<Abc> Ods => Set<Abc>();
    }


    public class Abc
    {
        public OrderZ? Id { get; set; }
    }
}