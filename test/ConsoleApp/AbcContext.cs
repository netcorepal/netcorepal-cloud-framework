using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.ConsoleApp
{
    public partial class AbcContext : EFContext
    {
        public AbcContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options,
            mediator, provider)
        {
        }


        public DbSet<Abc> Ods => Set<Abc>();
        
    }
    


    public class Abc
    {
        public OrderZ? Id { get; set; } = null!;
    }
}