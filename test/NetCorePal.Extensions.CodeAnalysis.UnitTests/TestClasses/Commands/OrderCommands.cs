using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 创建订单命令
/// </summary>
public record CreateOrderCommand(UserId UserId, decimal Amount, string ProductName) : ICommand<OrderId>;

/// <summary>
/// 取消订单命令
/// </summary>
public record CancelOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 确认订单命令
/// </summary>
public record ConfirmOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 订单支付命令
/// </summary>
public record OrderPaidCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 删除订单命令
/// </summary>
public record DeleteOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 更改订单名称命令
/// </summary>
public record ChangeOrderNameCommand(OrderId OrderId, string NewName) : ICommand;

/// <summary>
/// 更新订单状态命令
/// </summary>
public record UpdateOrderStatusCommand(OrderId OrderId, string Status) : ICommand
{
    // 无参构造函数用于测试
    public UpdateOrderStatusCommand() : this(new OrderId(Guid.NewGuid()), "Updated")
    {
    }
}
