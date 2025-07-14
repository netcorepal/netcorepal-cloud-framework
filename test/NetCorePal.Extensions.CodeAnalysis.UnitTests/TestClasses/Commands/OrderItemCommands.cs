using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 添加订单项命令
/// </summary>
public record AddOrderItemCommand(OrderId OrderId, string ProductName, int Quantity, decimal UnitPrice) : IRequest;

/// <summary>
/// 添加订单项命令处理器
/// </summary>
public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
{
    public Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        // 模拟处理逻辑
        var orderItemId = new OrderItemId(Guid.NewGuid());
        var orderItem = new OrderItem(orderItemId, request.ProductName, request.Quantity, request.UnitPrice);
        
        // 假设我们从某处获取了Order聚合根
        var order = new Order(request.OrderId, "测试订单", 100);
        
        // 调用聚合根的方法来添加订单项
        order.AddOrderItem(orderItem);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 更新订单项数量命令
/// </summary>
public record UpdateOrderItemQuantityCommand(OrderId OrderId, OrderItemId OrderItemId, int NewQuantity) : IRequest;

/// <summary>
/// 更新订单项数量命令处理器
/// </summary>
public class UpdateOrderItemQuantityCommandHandler : IRequestHandler<UpdateOrderItemQuantityCommand>
{
    public Task Handle(UpdateOrderItemQuantityCommand request, CancellationToken cancellationToken)
    {
        // 模拟处理逻辑
        // 假设我们从某处获取了Order聚合根
        var order = new Order(request.OrderId, "测试订单", 100);
        
        // 调用聚合根的方法来更新订单项数量（这会间接调用子实体的方法）
        order.UpdateOrderItemQuantity(request.OrderItemId, request.NewQuantity);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 更新订单项价格命令
/// </summary>
public record UpdateOrderItemPriceCommand(OrderId OrderId, OrderItemId OrderItemId, decimal NewUnitPrice) : IRequest;

/// <summary>
/// 更新订单项价格命令处理器
/// </summary>
public class UpdateOrderItemPriceCommandHandler : IRequestHandler<UpdateOrderItemPriceCommand>
{
    public Task Handle(UpdateOrderItemPriceCommand request, CancellationToken cancellationToken)
    {
        // 模拟处理逻辑
        // 假设我们从某处获取了Order聚合根
        var order = new Order(request.OrderId, "测试订单", 100);
        
        // 调用聚合根的方法来更新订单项价格（这会间接调用子实体的方法）
        order.UpdateOrderItemUnitPrice(request.OrderItemId, request.NewUnitPrice);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 移除订单项命令
/// </summary>
public record RemoveOrderItemCommand(OrderId OrderId, OrderItemId OrderItemId) : IRequest;

/// <summary>
/// 移除订单项命令处理器
/// </summary>
public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand>
{
    public Task Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        // 模拟处理逻辑
        // 假设我们从某处获取了Order聚合根
        var order = new Order(request.OrderId, "测试订单", 100);
        
        // 调用聚合根的方法来移除订单项（这会间接调用子实体的方法）
        order.RemoveOrderItem(request.OrderItemId);
        
        return Task.CompletedTask;
    }
}
