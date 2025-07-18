using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers;

/// <summary>
/// 订单控制器
/// </summary>
public class OrderController
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    public async Task<OrderId> CreateOrder(CreateOrderCommand command)
    {
        var orderId = await _mediator.Send(command);
        return orderId;
    }

    /// <summary>
    /// 支付订单
    /// </summary>
    public async Task PayOrder(Guid orderId)
    {
        await _mediator.Send(new OrderPaidCommand(new OrderId(orderId)));
    }

    /// <summary>
    /// 删除订单
    /// </summary>
    public async Task DeleteOrder(Guid orderId)
    {
        await _mediator.Send(new DeleteOrderCommand(new OrderId(orderId)));
    }

    /// <summary>
    /// 更改订单名称
    /// </summary>
    public async Task ChangeOrderName(Guid orderId, string newName)
    {
        await _mediator.Send(new ChangeOrderNameCommand(new OrderId(orderId), newName));
    }

    /// <summary>
    /// 获取订单
    /// </summary>
    public Task<Order> GetOrder(Guid orderId)
    {
        // 模拟获取订单
        return Task.FromResult<Order>(null!);
    }
}
