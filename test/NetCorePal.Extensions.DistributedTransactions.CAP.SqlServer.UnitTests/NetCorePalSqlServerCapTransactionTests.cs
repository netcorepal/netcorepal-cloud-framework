using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests;

public class NetCorePalSqlServerCapTransactionTests
{
    [Fact]
    public async Task DisposeAsync_Test()
    {
        // Arrange
        var mockDispatcher = new Mock<IDispatcher>();
        var transaction =
            new NetCorePalCapTransaction(mockDispatcher.Object);

        var mockDbContextTransaction = new Mock<IDbContextTransaction>();
        var disposed = false;
        mockDbContextTransaction.Setup(p => p.DisposeAsync()).Returns(new ValueTask()).Callback(() =>
        {
            disposed = true;
        });

        transaction.DbTransaction = mockDbContextTransaction.Object;
        await using (var dis = transaction)
        {
            await Task.Delay(1);
        }

        Assert.True(disposed);
    }
}