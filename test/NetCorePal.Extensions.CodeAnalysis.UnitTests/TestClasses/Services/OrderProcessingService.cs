using MediatR;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Services;

/// <summary>
/// 订单处理服务 - 另一个命令发送者示例
/// 这个服务处理复杂的订单业务流程
/// </summary>
public class OrderProcessingService
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(IMediator mediator, ILogger<OrderProcessingService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 处理订单创建流程
    /// </summary>
    public async Task<OrderId> ProcessNewOrder(UserId userId, decimal amount, string productName)
    {
        _logger.LogInformation("Processing new order for user {UserId}", userId);
        
        // 创建订单
        var createCommand = new CreateOrderCommand(userId, amount, productName);
        var orderId = await _mediator.Send(createCommand);
        
        // 自动确认订单（如果金额小于1000）
        if (amount < 1000)
        {
            await _mediator.Send(new ConfirmOrderCommand(orderId));
        }
        
        return orderId;
    }

    /// <summary>
    /// 批量处理订单支付
    /// </summary>
    public async Task ProcessBatchPayments(List<OrderId> orderIds)
    {
        foreach (var orderId in orderIds)
        {
            try
            {
                await _mediator.Send(new OrderPaidCommand(orderId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
                // 支付失败时取消订单
                await _mediator.Send(new CancelOrderCommand(orderId));
            }
        }
    }

    /// <summary>
    /// 清理已取消的订单
    /// </summary>
    public async Task CleanupCancelledOrders(List<OrderId> cancelledOrderIds)
    {
        foreach (var orderId in cancelledOrderIds)
        {
            await _mediator.Send(new DeleteOrderCommand(orderId));
        }
    }

    /// <summary>
    /// 处理订单修改请求
    /// </summary>
    public async Task ProcessOrderModification(OrderId orderId, string newName)
    {
        // 修改订单名称
        await _mediator.Send(new ChangeOrderNameCommand(orderId, newName));
        
        // 重新确认修改后的订单
        await _mediator.Send(new ConfirmOrderCommand(orderId));
    }
}
