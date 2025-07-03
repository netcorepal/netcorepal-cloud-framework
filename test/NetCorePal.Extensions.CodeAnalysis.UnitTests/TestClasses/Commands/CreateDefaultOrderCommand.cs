using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 创建默认订单命令
/// </summary>
public record CreateDefaultOrderCommand(OrderId OrderId) : ICommand<OrderId>;
