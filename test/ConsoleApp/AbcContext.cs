using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NetCorePal.ConsoleApp
{
    public partial class AbcContext : EFContext
    {
        public AbcContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options,
            mediator, provider)
        {
        }


        public DbSet<Order> Order { get; set; } = null!;
    }
    

    public class Order
    {
        public OrderId2? Id { get; set; }
    }
}