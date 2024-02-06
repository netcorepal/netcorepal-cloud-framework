using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="OrderId"></param>
    public record class OrderPaidCommand(long OrderId) : ICommand;
}
