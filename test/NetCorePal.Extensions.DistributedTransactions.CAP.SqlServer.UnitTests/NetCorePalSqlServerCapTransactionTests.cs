using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer.Diagnostics;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests;

[Obsolete("此测试类测试已废弃的功能。This test class tests obsolete functionality.")]
public class NetCorePalSqlServerCapTransactionTests
{
    [Fact]
    public async Task DisposeAsync_Test()
    {
        // Arrange
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDiagnosticProcessorObserver = new Mock<DiagnosticProcessorObserver>();
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockCapPublisher.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(IDispatcher))).Returns(mockDispatcher.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DiagnosticProcessorObserver)))
            .Returns(mockDiagnosticProcessorObserver.Object);
        var transaction =
            new NetCorePalSqlServerCapTransaction(mockDispatcher.Object, mockDiagnosticProcessorObserver.Object);

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