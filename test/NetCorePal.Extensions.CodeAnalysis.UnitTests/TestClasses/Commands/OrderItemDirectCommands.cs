using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 直接创建订单项命令（演示直接调用子实体构造函数）
/// </summary>
public record CreateOrderItemDirectCommand(string ProductName, int Quantity, decimal UnitPrice) : IRequest<OrderItemId>;

/// <summary>
/// 直接创建订单项命令处理器
/// </summary>
public class CreateOrderItemDirectCommandHandler : IRequestHandler<CreateOrderItemDirectCommand, OrderItemId>
{
    public Task<OrderItemId> Handle(CreateOrderItemDirectCommand request, CancellationToken cancellationToken)
    {
        // 直接调用OrderItem的构造函数
        var orderItemId = new OrderItemId(Guid.NewGuid());
        var orderItem = new OrderItem(orderItemId, request.ProductName, request.Quantity, request.UnitPrice);
        
        return Task.FromResult(orderItem.Id);
    }
}

/// <summary>
/// 直接更新订单项数量命令（演示直接调用子实体方法）
/// </summary>
public record UpdateOrderItemQuantityDirectCommand(OrderItemId OrderItemId, int NewQuantity) : IRequest;

/// <summary>
/// 直接更新订单项数量命令处理器
/// </summary>
public class UpdateOrderItemQuantityDirectCommandHandler : IRequestHandler<UpdateOrderItemQuantityDirectCommand>
{
    public Task Handle(UpdateOrderItemQuantityDirectCommand request, CancellationToken cancellationToken)
    {
        // 模拟获取OrderItem实例
        var orderItem = new OrderItem(request.OrderItemId, "测试产品", 1, 100);
        
        // 直接调用OrderItem的方法
        orderItem.UpdateQuantity(request.NewQuantity);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 直接更新订单项价格命令（演示直接调用子实体方法）
/// </summary>
public record UpdateOrderItemPriceDirectCommand(OrderItemId OrderItemId, decimal NewUnitPrice) : IRequest;

/// <summary>
/// 直接更新订单项价格命令处理器
/// </summary>
public class UpdateOrderItemPriceDirectCommandHandler : IRequestHandler<UpdateOrderItemPriceDirectCommand>
{
    public Task Handle(UpdateOrderItemPriceDirectCommand request, CancellationToken cancellationToken)
    {
        // 模拟获取OrderItem实例
        var orderItem = new OrderItem(request.OrderItemId, "测试产品", 1, 100);
        
        // 直接调用OrderItem的方法
        orderItem.UpdateUnitPrice(request.NewUnitPrice);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 直接移除订单项命令（演示直接调用子实体方法）
/// </summary>
public record RemoveOrderItemDirectCommand(OrderItemId OrderItemId) : IRequest;

/// <summary>
/// 直接移除订单项命令处理器
/// </summary>
public class RemoveOrderItemDirectCommandHandler : IRequestHandler<RemoveOrderItemDirectCommand>
{
    public Task Handle(RemoveOrderItemDirectCommand request, CancellationToken cancellationToken)
    {
        // 模拟获取OrderItem实例
        var orderItem = new OrderItem(request.OrderItemId, "测试产品", 1, 100);
        
        // 直接调用OrderItem的方法
        orderItem.Remove();
        
        return Task.CompletedTask;
    }
}
