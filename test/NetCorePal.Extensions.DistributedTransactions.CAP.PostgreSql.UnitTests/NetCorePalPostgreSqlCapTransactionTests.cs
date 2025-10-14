using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql.UnitTests;

[Obsolete("此测试类测试已废弃的功能。This test class tests obsolete functionality.")]
public class NetCorePalPostgreSqlCapTransactionTests
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
            new NetCorePalPostgreSqlCapTransaction(mockDispatcher.Object);

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