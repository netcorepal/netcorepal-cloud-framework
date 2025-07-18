using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 创建订单命令
/// </summary>
public record CreateOrderCommand(UserId UserId, decimal Amount, string ProductName) : ICommand<OrderId>;

/// <summary>
/// 订单支付命令
/// </summary>
public record OrderPaidCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 更改订单名称命令
/// </summary>
public record ChangeOrderNameCommand(OrderId OrderId, string NewName) : ICommand;

/// <summary>
/// 删除订单命令
/// </summary>
public record DeleteOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 更新订单项数量命令
/// </summary>
public record UpdateOrderItemQuantityCommand(OrderItemId OrderItemId, int NewQuantity) : ICommand;
