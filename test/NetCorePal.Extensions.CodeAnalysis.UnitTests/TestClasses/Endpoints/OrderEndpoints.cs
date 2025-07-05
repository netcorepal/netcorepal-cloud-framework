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
/// 取消订单请求
/// </summary>
public class CancelOrderRequest
{
    public OrderId OrderId { get; set; } = default!;
}

/// <summary>
/// 取消订单端点
/// </summary>
public class CancelOrderEndpoint : Endpoint<CancelOrderRequest>
{
    private readonly IMediator _mediator;

    public CancelOrderEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/orders/{orderId}/cancel");
        AllowAnonymous();
        Summary(s => s.Summary = "取消订单");
    }

    public override async Task HandleAsync(CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(request.OrderId);
        await _mediator.Send(command, cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}

/// <summary>
/// 确认订单请求
/// </summary>
public class ConfirmOrderRequest
{
    public OrderId OrderId { get; set; } = default!;
}

/// <summary>
/// 确认订单端点
/// </summary>
public class ConfirmOrderEndpoint : Endpoint<ConfirmOrderRequest>
{
    private readonly IMediator _mediator;

    public ConfirmOrderEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/orders/{orderId}/confirm");
        AllowAnonymous();
        Summary(s => s.Summary = "确认订单");
    }

    public override async Task HandleAsync(ConfirmOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmOrderCommand(request.OrderId);
        await _mediator.Send(command, cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}

/// <summary>
/// 创建默认订单端点 - 无参数的简单端点
/// </summary>
public class CreateDefaultOrderEndpoint : EndpointWithoutRequest<CreateOrderResponse>
{
    private readonly IMediator _mediator;

    public CreateDefaultOrderEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/orders/default");
        AllowAnonymous();
        Summary(s => s.Summary = "创建默认订单");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var command = new CreateDefaultOrderCommand();
        var orderId = await _mediator.Send(command, cancellationToken);
        
        await SendAsync(new CreateOrderResponse { OrderId = orderId }, cancellation: cancellationToken);
    }
}
