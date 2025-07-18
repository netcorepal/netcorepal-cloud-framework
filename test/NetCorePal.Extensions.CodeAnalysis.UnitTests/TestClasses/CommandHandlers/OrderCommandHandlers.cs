using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.CommandHandlers;

/// <summary>
/// 创建订单命令处理器
/// </summary>
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderId>
{
    public Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 创建订单聚合
        var order = new Order(new OrderId(Guid.NewGuid()), request.ProductName, request.Amount);
        
        // 这里应该保存到仓储，但为了测试我们只是返回ID
        return Task.FromResult(order.Id);
    }
}

/// <summary>
/// 订单支付命令处理器
/// </summary>
public class OrderPaidCommandHandler : ICommandHandler<OrderPaidCommand>
{
    public Task Handle(OrderPaidCommand request, CancellationToken cancellationToken)
    {
        // 在实际场景中，这里会从仓储加载订单聚合，然后调用业务方法
        // var order = await orderRepository.GetAsync(request.OrderId);
        
        // 为了让代码分析器能检测到命令与聚合方法的关系，我们创建一个临时的聚合实例并调用方法
        var order = new Order(request.OrderId, "Test Order", 10);
        order.MarkAsPaid();
        
        // await orderRepository.SaveAsync(order);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 删除订单命令处理器
/// </summary>
public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
{
    public Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // 模拟调用聚合方法
        // var order = await orderRepository.GetAsync(request.OrderId);
        
        // 为了让代码分析器能检测到命令与聚合方法的关系，我们创建一个临时的聚合实例并调用方法
        var order = new Order(request.OrderId, "Test Order", 10);
        order.SoftDelete();
        
        // await orderRepository.SaveAsync(order);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 更改订单名称命令处理器
/// </summary>
public class ChangeOrderNameCommandHandler : ICommandHandler<ChangeOrderNameCommand>
{
    public Task Handle(ChangeOrderNameCommand request, CancellationToken cancellationToken)
    {
        // 模拟调用聚合方法
        // var order = await orderRepository.GetAsync(request.OrderId);
        
        // 为了让代码分析器能检测到命令与聚合方法的关系，我们创建一个临时的聚合实例并调用方法
        var order = new Order(request.OrderId, "Old Name", 100);
        order.ChangeName(request.NewName);
        
        // await orderRepository.SaveAsync(order);
        
        return Task.CompletedTask;
    }
}
