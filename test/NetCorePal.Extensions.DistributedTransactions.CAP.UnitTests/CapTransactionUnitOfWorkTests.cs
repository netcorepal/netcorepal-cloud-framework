using DotNetCore.CAP;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public class CapTransactionUnitOfWorkTests
{
    [Fact]
    public async Task BeginTransactionAsync_Shold_Return_TransactionUnitOfWork_BeginTransactionAsync()
    {
        var mockTransactionUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();

        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork.Object,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        var beginTransactionAsync = false;
        mockTransactionUnitOfWork.Setup(p => p.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDbContextTransaction.Object).Callback(() => { beginTransactionAsync = true; });

        var transaction = await capTransactionUnitOfWork.BeginTransactionAsync();
        Assert.True(beginTransactionAsync);
        Assert.Equal(mockDbContextTransaction.Object, transaction);
    }

    [Fact]
    public async Task CommitAsync_Shold_Return_TransactionUnitOfWork_CommitAsync()
    {
        var mockTransactionUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();

        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork.Object,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        var commitAsync = false;
        mockTransactionUnitOfWork.Setup(p => p.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { commitAsync = true; });

        await capTransactionUnitOfWork.CommitAsync();
        Assert.True(commitAsync);
    }

    [Fact]
    public async Task RollbackAsync_Shold_Return_TransactionUnitOfWork_RollbackAsync()
    {
        var mockTransactionUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();

        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork.Object,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        var rollbackAsync = false;
        mockTransactionUnitOfWork.Setup(p => p.RollbackAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { rollbackAsync = true; });

        await capTransactionUnitOfWork.RollbackAsync();
        Assert.True(rollbackAsync);
    }

    [Fact]
    public async Task SaveChangesAsync_Shold_Return_TransactionUnitOfWork_SaveChangesAsync()
    {
        var mockTransactionUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();

        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork.Object,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        var saveChangeAsync = false;
        mockTransactionUnitOfWork.Setup(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => 10)
            .Callback(() => { saveChangeAsync = true; });

        var r = await capTransactionUnitOfWork.SaveChangesAsync();
        Assert.Equal(10, r);
        Assert.True(saveChangeAsync);
    }

    [Fact]
    public async Task SaveEntitiesAsync_Shold_Return_TransactionUnitOfWork_SaveEntitiesAsync()
    {
        var mockTransactionUnitOfWork = new Mock<ITransactionUnitOfWork>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();

        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork.Object,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        var saveEntitiesAsync = false;
        mockTransactionUnitOfWork.Setup(p => p.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => false)
            .Callback(() => { saveEntitiesAsync = true; });

        var r = await capTransactionUnitOfWork.SaveEntitiesAsync();
        Assert.False(r);
        Assert.True(saveEntitiesAsync);
    }

    [Fact]
    public void Set_CurrentTransaction_Shoul_Set_CapPublisher_Transaction()
    {
        
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockCapTransactionFactory = new Mock<ICapTransactionFactory>();
        var mockNetCorePalCapTransaction = new Mock<INetCorePalCapTransaction>();
        mockCapTransactionFactory.Setup(p => p.CreateCapTransaction())
            .Returns(mockNetCorePalCapTransaction.Object);
        var mockDbContextTransaction = new Mock<IDbContextTransaction>();
        var mockTransactionUnitOfWork = new MockInnerTransactionUnitOfWork(mockDbContextTransaction.Object);
        mockNetCorePalCapTransaction.Setup(p => p.DbTransaction)
            .Returns(mockDbContextTransaction.Object);


        CapTransactionUnitOfWork capTransactionUnitOfWork = new CapTransactionUnitOfWork(
            mockTransactionUnitOfWork,
            mockCapPublisher.Object,
            mockCapTransactionFactory.Object);

        var capPublisherTransactionSetted = false;
        mockCapPublisher.SetupSet(p => p.Transaction = It.IsAny<ICapTransaction>())
            .Callback(() => { capPublisherTransactionSetted = true; });

        // Arrange
        Assert.Null(capTransactionUnitOfWork.CurrentTransaction);

        capTransactionUnitOfWork.CurrentTransaction = mockDbContextTransaction.Object;
        Assert.True(capPublisherTransactionSetted);
        Assert.NotNull(capTransactionUnitOfWork.CurrentTransaction);
        Assert.IsType<NetCorePalCapEFDbTransaction>(capTransactionUnitOfWork.CurrentTransaction);
    }
}

public class MockInnerTransactionUnitOfWork(IDbContextTransaction dbContextTransaction) : ITransactionUnitOfWork
{
    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return dbContextTransaction.DisposeAsync();
    }



    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dbContextTransaction);
    }

    public IDbContextTransaction? CurrentTransaction { get; set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}