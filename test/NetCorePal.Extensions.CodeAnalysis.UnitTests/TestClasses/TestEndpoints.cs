using FastEndpoints;
using MediatR;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;



public record TestEndpointRequest();

public class TestEndpoint(IMediator mediator) : Endpoint<TestEndpointRequest>
{
    public override void Configure()
    {
        Get("/test-endpoint");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TestEndpointRequest req, CancellationToken ct)
    {
        // 这里可以调用 Mediator 发送命令或事件
        await mediator.Send(new EndpointCommandWithOutResult("TestName"), ct);
        await SendOkAsync(ct);
    }
}

public record TestEndpointWithResultRequest(string Name);
public record TestEndpointWithResultResponse(string Result);


public class TestEndpointWithResult(IMediator mediator) : Endpoint<TestEndpointWithResultRequest, TestEndpointWithResultResponse>
{
    public override void Configure()
    {
        Get("/test-endpoint-with-result");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TestEndpointWithResultRequest req, CancellationToken ct)
    {
        // 这里可以调用 Mediator 发送命令或事件
        var result = await mediator.Send(new EndpointCommandWithResult(req.Name), ct);
        await SendAsync(new TestEndpointWithResultResponse(result), cancellation: ct);
    }
}

public class TestEndpointWithOutRequest(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/test-endpoint-with-out-result");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await mediator.Send(new EndpointCommandWithOutResult("TestName"));
        await SendOkAsync(ct);
    }
}

public record TestEndpointWithOutRequestResponse(string Result);

public class TestEndpointWithOutRequestResponseEndpoint(IMediator mediator) : EndpointWithoutRequest<TestEndpointWithOutRequestResponse>
{
    public override void Configure()
    {
        Get("/test-endpoint-with-out-request-response");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new TestEndpointWithOutRequestResponse("Test Result");
        await mediator.Send(new RecordCommandWithOutResult("TestName"), ct);
        await SendAsync(response, cancellation: ct);
    }
}


public class TestEndpointWitoutCommand : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/test-endpoint-without-command");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 这个端点没有发出任何命令
        await SendOkAsync(ct);
    }
}
