using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Moq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql.UnitTests;

[Obsolete("此测试类测试已废弃的功能。This test class tests obsolete functionality.")]
public class MySqlCapTransactionFactoryTests
{
    [Fact]
    public void CreateCapTransaction_Test()
    {
        var mockCapPublisher = new Mock<ICapPublisher>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockCapPublisher.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockDispatcher = new Mock<IDispatcher>();
        mockServiceProvider.Setup(p => p.GetService(typeof(IDispatcher))).Returns(mockDispatcher.Object);

        MySqlCapTransactionFactory factory = new MySqlCapTransactionFactory(mockCapPublisher.Object);
        var transaction = factory.CreateCapTransaction();
        Assert.NotNull(transaction);
        Assert.IsType<NetCorePalMySqlCapTransaction>(transaction);
    }
}