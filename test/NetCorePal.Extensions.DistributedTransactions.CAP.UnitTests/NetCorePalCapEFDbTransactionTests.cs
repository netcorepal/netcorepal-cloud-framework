using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public class NetCorePalCapEFDbTransactionTests
{
    [Fact]
    public async Task CreateCapEFDbTransaction_Test()
    {
        var transactionId = Guid.NewGuid();
        var mockDbTransaction = new Mock<DbTransaction>();
        var mockDbContextTransaction = new Mock<IInfraDbContextTransaction>();
        mockDbContextTransaction.Setup(p => p.Instance).Returns(mockDbTransaction.Object);
        mockDbContextTransaction.Setup(p => p.TransactionId).Returns(transactionId);
        Mock<INetCorePalCapTransaction> capTransaction = new Mock<INetCorePalCapTransaction>();
        capTransaction.Setup(p => p.DbTransaction)
            .Returns(mockDbContextTransaction.Object);


        NetCorePalCapEFDbTransaction transaction =
            new NetCorePalCapEFDbTransaction(capTransaction.Object);

        Assert.Equal(transactionId, transaction.TransactionId);
        Assert.Equal(mockDbTransaction.Object, transaction.Instance);

        #region Dispose

        var disposed = false;
        capTransaction.Setup(p => p.Dispose()).Callback(() => { disposed = true; });
        transaction.Dispose();
        Assert.True(disposed);

        #endregion

        #region Commit

        var committed = false;
        capTransaction.Setup(p => p.Commit()).Callback(() => { committed = true; });
        transaction.Commit();
        Assert.True(committed);

        #endregion


        #region Rollback

        var rollbacked = false;
        capTransaction.Setup(p => p.Rollback()).Callback(() => { rollbacked = true; });
        transaction.Rollback();
        Assert.True(rollbacked);

        #endregion

        #region CommitAsync

        var commitAsync = false;
        capTransaction.Setup(p => p.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => { commitAsync = true; });
        await transaction.CommitAsync();
        Assert.True(commitAsync);

        #endregion

        #region RollbackAsync

        var rollbackAsync = false;
        capTransaction.Setup(p => p.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => { rollbackAsync = true; });
        await transaction.RollbackAsync();
        Assert.True(rollbackAsync);

        #endregion

        #region DisposeAsync

        var disposeAsync = false;
        capTransaction.Setup(p => p.DisposeAsync())
            .Returns(new ValueTask())
            .Callback(() => { disposeAsync = true; });
        await transaction.DisposeAsync();
        Assert.True(disposeAsync);

        #endregion
    }
}

public interface IInfraDbContextTransaction : IDbContextTransaction, IInfrastructure<DbTransaction>
{
}