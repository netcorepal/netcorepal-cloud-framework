using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Price"></param>
    /// <param name="Count"></param>
    public record CreateOrderCommand(string Name, int Price, int Count) : ICommand<OrderId>;
}
