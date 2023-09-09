using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Application.Commands
{
    public record CreateOrderCommand(string Name, int Price, int Count) : ICommand<OrderId>;
}
