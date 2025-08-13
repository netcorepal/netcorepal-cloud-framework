using FastEndpoints;
using MediatR;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 测试没有发出任何命令的Endpoint
/// </summary>
public class TestEmptyEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/empty-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 只返回固定数据，不发出命令
        await SendOkAsync("No commands sent", ct);
    }
}

/// <summary>
/// 测试复杂场景的Endpoint
/// </summary>
public record ComplexEndpointRequest(string Action, string Data);

public class TestComplexEndpoint(IMediator mediator) : Endpoint<ComplexEndpointRequest>
{
    public override void Configure()
    {
        Post("/complex-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ComplexEndpointRequest req, CancellationToken ct)
    {
        switch (req.Action.ToLower())
        {
            case "create":
                await mediator.Send(new RecordCommandWithResult($"Creating: {req.Data}"), ct);
                break;
            case "update":
                await mediator.Send(new ClassCommandWithOutResult { Name = $"Updating: {req.Data}" }, ct);
                break;
            case "delete":
                await mediator.Send(new RecordCommandWithResult($"Deleting: {req.Data}"), ct);
                await mediator.Send(new ClassCommandWithOutResult { Name = "Cleanup" }, ct);
                break;
            default:
                // 默认情况不发送命令
                break;
        }
        
        await SendOkAsync(ct);
    }
}

/// <summary>
/// 测试批量操作的Endpoint
/// </summary>
public record BatchEndpointRequest(string[] Items);

public class TestBatchEndpoint(IMediator mediator) : Endpoint<BatchEndpointRequest>
{
    public override void Configure()
    {
        Post("/batch-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(BatchEndpointRequest req, CancellationToken ct)
    {
        // 批量处理，每个项目发出一个命令
        var tasks = req.Items.Select(item => 
            mediator.Send(new RecordCommandWithResult($"Batch: {item}"), ct));
        
        await Task.WhenAll(tasks);
        await SendOkAsync(ct);
    }
}

/// <summary>
/// 测试异步流处理的Endpoint
/// </summary>
public record StreamEndpointRequest(int Count);

public class TestStreamEndpoint(IMediator mediator) : Endpoint<StreamEndpointRequest>
{
    public override void Configure()
    {
        Post("/stream-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(StreamEndpointRequest req, CancellationToken ct)
    {
        for (int i = 0; i < req.Count; i++)
        {
            await mediator.Send(new RecordCommandWithResult($"Stream Item {i}"), ct);
            
            if (i % 2 == 0)
            {
                await mediator.Send(new ClassCommandWithOutResult { Name = $"Even Item {i}" }, ct);
            }
        }
        
        await SendOkAsync(ct);
    }
}

/// <summary>
/// 测试带验证的Endpoint
/// </summary>
public record ValidatedEndpointRequest(string Name, int Age);

public class TestValidatedEndpoint(IMediator mediator) : Endpoint<ValidatedEndpointRequest>
{
    public override void Configure()
    {
        Post("/validated-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ValidatedEndpointRequest req, CancellationToken ct)
    {
        // 验证逻辑
        if (string.IsNullOrEmpty(req.Name))
        {
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (req.Age < 18)
        {
            await mediator.Send(new ClassCommandWithOutResult { Name = "Minor Registration" }, ct);
        }
        else
        {
            await mediator.Send(new RecordCommandWithResult("Adult Registration"), ct);
        }
        
        await SendOkAsync(ct);
    }
}
