using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands;

public record DeleteOrderCommand(OrderId OrderId) : ICommand;

public class DeleteOrderCommandHandler(IOrderRepository orderRepository) : ICommandHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(request.OrderId, cancellationToken) ??
                    throw new KnownException($"未找到订单，OrderId = {request.OrderId}");
        order.SoftDelete();
    }
}