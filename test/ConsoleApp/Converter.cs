using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
namespace NetCorePal.ConsoleApp
{
   
    public partial class OrderId22ValueConverter : ValueConverter<NetCorePal.ConsoleApp.OrderId2, Int64>
    {
        public  OrderId22ValueConverter() : base(p => p.Id, p => new NetCorePal.ConsoleApp.OrderId2(p)) { }
    }

}