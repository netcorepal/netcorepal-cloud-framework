using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.ConsoleApp
{
    public partial class AbcContext(DbContextOptions options, IMediator mediator) : AppDbContextBase(options,
        mediator)
    {
        public DbSet<Abc> Ods => Set<Abc>();
    }


    public class Abc
    {
        public OrderZ? Id { get; set; }
    }
}