using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer.Diagnostics;
using DotNetCore.CAP.Transport;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests;

public class SqlServerCapTransactionFactoryTests
{
    [Fact]
    public void CreateCapTransaction_Test()
    {
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockCapPublisher.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockDispatcher = new Mock<IDispatcher>();
        var mockDiagnosticProcessorObserver = new Mock<DiagnosticProcessorObserver>();
        mockServiceProvider.Setup(p => p.GetService(typeof(IDispatcher))).Returns(mockDispatcher.Object);
        mockServiceProvider.Setup(p => p.GetService(typeof(DiagnosticProcessorObserver))).Returns(mockDiagnosticProcessorObserver.Object);
        
        SqlServerCapTransactionFactory factory = new SqlServerCapTransactionFactory(mockCapPublisher.Object);
        var transaction = factory.CreateCapTransaction();
        Assert.NotNull(transaction);
        Assert.IsType<NetCorePalSqlServerCapTransaction>(transaction);
    }
    
}