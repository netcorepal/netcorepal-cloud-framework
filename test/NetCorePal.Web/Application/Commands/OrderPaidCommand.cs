using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    public record class OrderPaidCommand(long OrderId) : ICommand;
}
