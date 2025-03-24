using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Behaviors;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;

public class CommandUnitOfWorkBehaviorTests
{
    [Fact]
    public async Task Handle_Without_Errors()
    {
        var mockUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockTransaction = new Mock<IDbContextTransaction>();
        List<string> logs = new List<string>();
        mockUnitOfWork.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object)
            .Callback(() => logs.Add("BeginTransactionAsync"));
        mockUnitOfWork.SetupSet(p => p.CurrentTransaction = It.IsAny<IDbContextTransaction>())
            .Callback(() => logs.Add("CurrentTransaction"));
        mockUnitOfWork.Setup(p => p.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("SaveEntitiesAsync"))
            .ReturnsAsync(true);
        mockUnitOfWork.Setup(p => p.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("CommitAsync"));
        mockTransaction.Setup(p=>p.DisposeAsync())
            .Callback(() => logs.Add("DisposeAsync"));
        var w = new CommandUnitOfWorkBehavior<TestCommand, string>(mockUnitOfWork.Object);
        var response = await w.Handle(new TestCommand(),
            () => Task.FromResult("hello"), CancellationToken.None);
        
        Assert.Equal("BeginTransactionAsync", logs[0]);
        Assert.Equal("CurrentTransaction", logs[1]);
        Assert.Equal("SaveEntitiesAsync", logs[2]);
        Assert.Equal("CommitAsync", logs[3]);
        Assert.Equal("DisposeAsync", logs[4]);
        Assert.Equal("hello", response);
    }


    [Fact]
    public async Task Handle_With_Next_Error_And_Rollback()
    {
        var mockUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockTransaction = new Mock<IDbContextTransaction>();
        List<string> logs = new List<string>();
        mockUnitOfWork.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object)
            .Callback(() => logs.Add("BeginTransactionAsync"));
        mockUnitOfWork.SetupSet(p => p.CurrentTransaction = It.IsAny<IDbContextTransaction>())
            .Callback(() => logs.Add("CurrentTransaction"));
        mockUnitOfWork.Setup(p => p.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("SaveEntitiesAsync"))
            .ReturnsAsync(true);
        mockUnitOfWork.Setup(p => p.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("CommitAsync"));

        mockUnitOfWork.Setup(p => p.RollbackAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("RollbackAsync"));
        mockTransaction.Setup(p=>p.DisposeAsync())
            .Callback(() => logs.Add("DisposeAsync"));
        var w = new CommandUnitOfWorkBehavior<TestCommand, string>(mockUnitOfWork.Object);
        await Assert.ThrowsAsync<Exception>(() => w.Handle(new TestCommand(),
            () => throw new Exception("error"), CancellationToken.None));
        Assert.Equal("BeginTransactionAsync", logs[0]);
        Assert.Equal("CurrentTransaction", logs[1]);
        Assert.Equal("RollbackAsync", logs[2]);
        Assert.Equal("DisposeAsync", logs[3]);
    }

    [Fact]
    public async Task Handle_With_SaveEntitiesAsync_Error_Errors_And_Rollback()
    {
        var mockUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockTransaction = new Mock<IDbContextTransaction>();
        List<string> logs = new List<string>();
        mockUnitOfWork.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object)
            .Callback(() => logs.Add("BeginTransactionAsync"));
        mockUnitOfWork.SetupSet(p => p.CurrentTransaction = It.IsAny<IDbContextTransaction>())
            .Callback(() => logs.Add("CurrentTransaction"));
        mockUnitOfWork.Setup(p => p.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => throw new Exception("SaveEntitiesAsync Error"))
            .ReturnsAsync(true);
        mockUnitOfWork.Setup(p => p.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("CommitAsync"));

        mockUnitOfWork.Setup(p => p.RollbackAsync(It.IsAny<CancellationToken>()))
            .Callback(() => logs.Add("RollbackAsync"));
        
        mockTransaction.Setup(p=>p.DisposeAsync())
            .Callback(() => logs.Add("DisposeAsync"));

        var w = new CommandUnitOfWorkBehavior<TestCommand, string>(mockUnitOfWork.Object);
        await Assert.ThrowsAsync<Exception>(() => w.Handle(new TestCommand(),
            () =>
            {
                logs.Add("hello");
                return Task.FromResult("hello");
            }, CancellationToken.None));
        Assert.Equal("BeginTransactionAsync", logs[0]);
        Assert.Equal("CurrentTransaction", logs[1]);
        Assert.Equal("hello", logs[2]);
        Assert.Equal("RollbackAsync", logs[3]);
        Assert.Equal("DisposeAsync", logs[4]);
    }
}

public class TestCommand : ICommand<string>
{
    public string Execute()
    {
        return "Hello";
    }
}