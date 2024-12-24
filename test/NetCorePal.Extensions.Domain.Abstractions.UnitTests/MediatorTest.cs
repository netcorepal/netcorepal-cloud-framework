using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class MediatorTest
{
    [Fact]
    public async Task Test()
    {
        var services = new ServiceCollection();
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(MediatorTest).Assembly)
            .AddOpenBehavior(typeof(TestCommandBehavior<,>));
        });
        //services.AddTransient<TestCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Send(new TestCommand());
    }
    
    
}

public class TestCommand : IRequest { }
    
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult("TestCommand");
    }
}
    
public class TestCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}