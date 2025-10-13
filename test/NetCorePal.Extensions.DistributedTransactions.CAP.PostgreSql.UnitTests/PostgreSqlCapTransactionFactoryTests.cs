using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql.UnitTests;

public class PostgreSqlCapTransactionFactoryTests
{
    [Fact]
    public void CreateCapTransaction_Test()
    {
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockCapPublisher.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockDispatcher = new Mock<IDispatcher>();
        mockServiceProvider.Setup(p => p.GetService(typeof(IDispatcher))).Returns(mockDispatcher.Object);

        PostgreSqlCapTransactionFactory factory = new PostgreSqlCapTransactionFactory(mockCapPublisher.Object);
        var transaction = factory.CreateCapTransaction();
        Assert.NotNull(transaction);
        Assert.IsType<NetCorePalCapTransaction>(transaction);
    }
}