using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    public record DeliverGoodsCommand(OrderId OrderId) : ICommand<DeliverRecordId>;
}
