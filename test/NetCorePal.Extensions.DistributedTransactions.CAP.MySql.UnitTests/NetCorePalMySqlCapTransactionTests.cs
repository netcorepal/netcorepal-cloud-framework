using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql.UnitTests;

public class NetCorePalMySqlCapTransactionTests
{
    [Fact]
    public async Task DisposeAsync_Test()
    {
        // Arrange
        var mockDispatcher = new Mock<IDispatcher>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockCapPublisher.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(IDispatcher))).Returns(mockDispatcher.Object);
        var transaction =
            new NetCorePalMySqlCapTransaction(mockDispatcher.Object);

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