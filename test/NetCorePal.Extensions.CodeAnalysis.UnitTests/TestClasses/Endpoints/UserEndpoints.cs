using FastEndpoints;
using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Endpoints;

/// <summary>
/// 创建用户请求
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 创建用户响应
/// </summary>
public class CreateUserResponse
{
    public UserId UserId { get; set; } = default!;
}

/// <summary>
/// 创建用户端点
/// </summary>
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly IMediator _mediator;

    public CreateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        Summary(s => s.Summary = "创建用户");
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Name, request.Email);
        var userId = await _mediator.Send(command, cancellationToken);
        
        await SendAsync(new CreateUserResponse { UserId = userId }, cancellation: cancellationToken);
    }
}

/// <summary>
/// 激活用户请求
/// </summary>
public class ActivateUserRequest
{
    public UserId UserId { get; set; } = default!;
}

/// <summary>
/// 激活用户端点
/// </summary>
public class ActivateUserEndpoint : Endpoint<ActivateUserRequest>
{
    private readonly IMediator _mediator;

    public ActivateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/users/{userId}/activate");
        AllowAnonymous();
        Summary(s => s.Summary = "激活用户");
    }

    public override async Task HandleAsync(ActivateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(request.UserId);
        await _mediator.Send(command, cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}

/// <summary>
/// 禁用用户请求
/// </summary>
public class DeactivateUserRequest
{
    public UserId UserId { get; set; } = default!;
}

/// <summary>
/// 禁用用户端点
/// </summary>
public class DeactivateUserEndpoint : Endpoint<DeactivateUserRequest>
{
    private readonly IMediator _mediator;

    public DeactivateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/users/{userId}/deactivate");
        AllowAnonymous();
        Summary(s => s.Summary = "禁用用户");
    }

    public override async Task HandleAsync(DeactivateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(request.UserId);
        await _mediator.Send(command, cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}
