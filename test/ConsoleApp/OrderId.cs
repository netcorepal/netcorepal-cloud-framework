using NetCorePal.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.ConsoleApp
{
    public record OrderId222(long Id) : IEntityId { }
    public record OrderId2(int Id) : IEntityId { }
}
