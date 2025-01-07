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
    [Fact]
    public async Task CommandLockFailedException_Should_Throw_When_One_Key_Lock_Failed()
    {
        var mockDistributedLock = new Mock<IDistributedLock>();
        mockDistributedLock.Setup(p =>
                p.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ILockSynchronizationHandler));

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
        var cmd = new TestCommand();
        var ex = await Assert.ThrowsAsync<CommandLockFailedException>(
            async () => await mediator.Send(cmd));
        Assert.Equal(cmd.CommandId, ex.FailedKey);
    }

    [Fact]
    public async Task CommandLockFailedException_Should_Not_Throw_When_One_Key_Lock_Success()
    {
        var mockDistributedLock = new Mock<IDistributedLock>();
        int disposeCount = 0;
        var mockLockSynchronizationHandler = new Mock<ILockSynchronizationHandler>();
        mockLockSynchronizationHandler.Setup(p => p.DisposeAsync())
            .Callback(() => disposeCount++);
        mockDistributedLock.Setup(p =>
                p.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLockSynchronizationHandler.Object);

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
        Assert.Equal(7, disposeCount);
    }

    [Fact]
    public async Task CommandLockFailedException_Should_Throw_When_Any_Key_Lock_Failed()
    {
        var mockDistributedLock = new Mock<IDistributedLock>();

        int disposeCount = 0;
        var mockLockSynchronizationHandler = new Mock<ILockSynchronizationHandler>();
        mockLockSynchronizationHandler.Setup(p => p.DisposeAsync())
            .Callback(() => disposeCount++);
        mockDistributedLock.Setup(p =>
                p.TryAcquireAsync(It.IsNotIn("key3"), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLockSynchronizationHandler.Object);
        mockDistributedLock.Setup(p =>
                p.TryAcquireAsync(It.IsIn("key3"), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ILockSynchronizationHandler));

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
        var s = scope.ServiceProvider.GetRequiredService<ICommandLock<TestCommand2>>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var ex = await Assert.ThrowsAsync<CommandLockFailedException>(
            async () => await mediator.Send(new TestCommand2()));
        Assert.Equal("key3", ex.FailedKey);
        Assert.Equal(2, disposeCount);
    }

    [Fact]
    public async Task CommandLockFailedException_Should_Not_Throw_When_All_Key_Lock_Success()
    {
        var mockDistributedLock = new Mock<IDistributedLock>();
        int disposeCount = 0;
        var mockLockSynchronizationHandler = new Mock<ILockSynchronizationHandler>();
        mockLockSynchronizationHandler.Setup(p => p.DisposeAsync())
            .Callback(() => disposeCount++);
        mockDistributedLock.Setup(p =>
                p.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLockSynchronizationHandler.Object);

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
        var s = scope.ServiceProvider.GetRequiredService<ICommandLock<TestCommand2>>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var cmd = new TestCommand2();
        var r = await mediator.Send(cmd);
        Assert.Equal(5, disposeCount);
        Assert.Equal(cmd.CommandId + "-handled", r);
    }


    public class TestCommand : ICommand
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
    }

    public class CommandLock : ICommandLock<TestCommand>
    {
        public Task<CommandLockSettings> GetLockKeysAsync(TestCommand command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings(command.CommandId));
        }
    }

    public class TestCommandHandler(IMediator mediator) : ICommandHandler<TestCommand>
    {
        public async Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            var cmd = new TestCommand3() { CommandId = request.CommandId };
            await mediator.Send(cmd, cancellationToken);
            await mediator.Send(cmd, cancellationToken); // 这行用来测试锁释放后，重入是否会重新锁的情况
        }
    }


    public class TestCommand2 : ICommand<string>
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
    }

    public class CommandLock2 : ICommandLock<TestCommand2>
    {
        public Task<CommandLockSettings> GetLockKeysAsync(TestCommand2 command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings(["key1", "key2", "key3"]));
        }
    }

    public class TestCommandHandler2(IMediator mediator) : ICommandHandler<TestCommand2, string>
    {
        public async Task<string> Handle(TestCommand2 request, CancellationToken cancellationToken)
        {
            TestCommand3 cmd = new();
            await mediator.Send(cmd, cancellationToken);
            await mediator.Send(cmd, cancellationToken); // 这行用来测试锁释放后，重入是否会重新锁的情况
            return request.CommandId + "-handled";
        }
    }

    public class TestCommand3 : ICommand<string>
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString();
    }

    public class CommandLock3 : ICommandLock<TestCommand3>
    {
        public Task<CommandLockSettings> GetLockKeysAsync(TestCommand3 command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandLockSettings([command.CommandId, "key1", "key2", "key3"]));
        }
    }

    public class TestCommandHandler3 : ICommandHandler<TestCommand3, string>
    {
        public Task<string> Handle(TestCommand3 request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.CommandId + "-handled");
        }
    }
}