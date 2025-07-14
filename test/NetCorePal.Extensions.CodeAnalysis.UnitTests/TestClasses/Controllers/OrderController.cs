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

    /// <summary>
    /// 添加订单项
    /// </summary>
    public async Task AddOrderItem(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        await _mediator.Send(new AddOrderItemCommand(new OrderId(orderId), productName, quantity, unitPrice));
    }

    /// <summary>
    /// 更新订单项数量
    /// </summary>
    public async Task UpdateOrderItemQuantity(Guid orderId, Guid orderItemId, int newQuantity)
    {
        await _mediator.Send(new UpdateOrderItemQuantityCommand(new OrderId(orderId), new OrderItemId(orderItemId), newQuantity));
    }

    /// <summary>
    /// 更新订单项价格
    /// </summary>
    public async Task UpdateOrderItemPrice(Guid orderId, Guid orderItemId, decimal newUnitPrice)
    {
        await _mediator.Send(new UpdateOrderItemPriceCommand(new OrderId(orderId), new OrderItemId(orderItemId), newUnitPrice));
    }

    /// <summary>
    /// 移除订单项
    /// </summary>
    public async Task RemoveOrderItem(Guid orderId, Guid orderItemId)
    {
        await _mediator.Send(new RemoveOrderItemCommand(new OrderId(orderId), new OrderItemId(orderItemId)));
    }

    /// <summary>
    /// 直接创建订单项（演示直接调用子实体构造函数）
    /// </summary>
    public async Task<OrderItemId> CreateOrderItemDirect(string productName, int quantity, decimal unitPrice)
    {
        return await _mediator.Send(new CreateOrderItemDirectCommand(productName, quantity, unitPrice));
    }

    /// <summary>
    /// 直接更新订单项数量（演示直接调用子实体方法）
    /// </summary>
    public async Task UpdateOrderItemQuantityDirect(Guid orderItemId, int newQuantity)
    {
        await _mediator.Send(new UpdateOrderItemQuantityDirectCommand(new OrderItemId(orderItemId), newQuantity));
    }

    /// <summary>
    /// 直接更新订单项价格（演示直接调用子实体方法）
    /// </summary>
    public async Task UpdateOrderItemPriceDirect(Guid orderItemId, decimal newUnitPrice)
    {
        await _mediator.Send(new UpdateOrderItemPriceDirectCommand(new OrderItemId(orderItemId), newUnitPrice));
    }

    /// <summary>
    /// 直接移除订单项（演示直接调用子实体方法）
    /// </summary>
    public async Task RemoveOrderItemDirect(Guid orderItemId)
    {
        await _mediator.Send(new RemoveOrderItemDirectCommand(new OrderItemId(orderItemId)));
    }
}
