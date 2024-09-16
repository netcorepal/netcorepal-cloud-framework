using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands;

/// <summary>
/// 
/// </summary>
/// <param name="OrderId"></param>
/// <param name="NewName"></param>
public record SetOrderItemNameCommand(OrderId OrderId,string NewName) : ICommand;

/// <summary>
/// 
/// </summary>
/// <param name="orderRepository"></param>
public class SetOrderItemNameCommandHandler(IOrderRepository orderRepository) : ICommandHandler<SetOrderItemNameCommand>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="KnownException"></exception>
    public async Task Handle(SetOrderItemNameCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(command.OrderId, cancellationToken);
        if (order == null)
        {
            throw new KnownException("Order not found");
        }

        order.ChangeItemName(command.NewName);
    }

    
}