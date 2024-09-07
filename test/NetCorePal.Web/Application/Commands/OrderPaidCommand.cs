using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="OrderId"></param>
    public record class OrderPaidCommand(OrderId OrderId) : ICommand;
}
