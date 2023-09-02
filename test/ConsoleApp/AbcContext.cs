using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore;
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


        public DbSet<Order> Order2 { get; set; } = null!;
        
        public DbSet<Abc> Abc2 { get; set; } = null!;
    }
    

    public class Order
    {
        public AbcId? Id { get; set; } 
    }

    public class Abc
    {
        public OrderZ? Id { get; set; } = null!;
    }
}