using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore.CommandLocks;
using NetCorePal.Extensions.Primitives;
using MediatR;
using Moq;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedLocks;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class CommandLockBehaviorTest
{
    public class TestCommand : ICommand
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
    }

    public class CommandLock : ICommandLock<TestCommand>
    {
        public Task<CommandLockSettings> GetCommandLockOptionsAsync(TestCommand command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings(command.CommandId));
        }
        
        
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Unit.Value);
        }
    }

    [Fact]
    public async Task Test()
    {
        var mockDistributedLock = new Mock<IDistributedLock>();
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedLock>(mockDistributedLock.Object);


        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssembly(typeof(CommandLockBehaviorTest).Assembly);
            c.AddOpenBehavior(typeof(CommandLockBehavior<,>));
        });

        services.AddCommandLocks(typeof(CommandLockBehaviorTest).Assembly);


        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var s = scope.ServiceProvider.GetRequiredService<ICommandLock<TestCommand>>();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new TestCommand());
    }
}