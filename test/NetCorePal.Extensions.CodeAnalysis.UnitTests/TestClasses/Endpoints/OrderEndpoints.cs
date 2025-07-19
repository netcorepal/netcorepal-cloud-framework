using FastEndpoints;
using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Endpoints;

/// <summary>
/// 创建订单请求
/// </summary>
public class CreateOrderRequest
{
    public UserId UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string ProductName { get; set; } = string.Empty;
}

/// <summary>
/// 创建订单响应
/// </summary>
public class CreateOrderResponse
{
    public OrderId OrderId { get; set; } = default!;
}

/// <summary>
/// 创建订单端点
/// </summary>
public class CreateOrderEndpoint : Endpoint<CreateOrderRequest, CreateOrderResponse>
{
    private readonly IMediator _mediator;

    public CreateOrderEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
        Summary(s => s.Summary = "创建订单");
    }

    public override async Task HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(request.UserId, request.Amount, request.ProductName);
        var orderId = await _mediator.Send(command, cancellationToken);
        
        await SendAsync(new CreateOrderResponse { OrderId = orderId }, cancellation: cancellationToken);
    }
}

/// <summary>
/// 支付订单请求
/// </summary>
public class PayOrderRequest
{
    public OrderId OrderId { get; set; } = default!;
}

/// <summary>
/// 支付订单端点
/// </summary>
public class PayOrderEndpoint : Endpoint<PayOrderRequest>
{
    private readonly IMediator _mediator;

    public PayOrderEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/orders/{orderId}/pay");
        AllowAnonymous();
        Summary(s => s.Summary = "支付订单");
    }

    public override async Task HandleAsync(PayOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new OrderPaidCommand(request.OrderId);
        await _mediator.Send(command, cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}
