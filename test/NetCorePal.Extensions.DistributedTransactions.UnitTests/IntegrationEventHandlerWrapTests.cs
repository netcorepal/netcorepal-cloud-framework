namespace NetCorePal.Extensions.DistributedTransactions.UnitTests;

public class IntegrationEventHandlerWrapTests
{
    [Fact]
    public async Task HanlderFilterAsyncTest()
    {
        bool handlerCalled = false;
        var handlerMock = new Mock<IIntegrationEventHandler<TestIntegrationEvent>>();
        handlerMock.Setup(p =>
                p.HandleAsync(It.IsAny<TestIntegrationEvent>(),
                    It.IsAny<CancellationToken>()))
            .Callback<TestIntegrationEvent, CancellationToken>(
                (data, cancellationToken) =>
                {
                    handlerCalled = true;
                    Assert.NotNull(data);
                    Assert.Equal("abc", data.Name);
                    Assert.Equal(10, data.Age);
                })
            .Returns(Task.CompletedTask);
        var mockFilter1 = new Mock<IIntegrationEventHandlerFilter>();

        bool filter1Called = false;
        mockFilter1.Setup(p =>
                p.HandleAsync(It.IsAny<IntegrationEventHandlerContext>(),
                    It.IsAny<IntegrationEventHandlerDelegate>()))
            .Callback<IntegrationEventHandlerContext, IntegrationEventHandlerDelegate>((context, next) =>
            {
                filter1Called = true;
                Assert.NotNull(context);
                Assert.NotNull(context.Data);
                Assert.NotNull(context.Headers);
            }).Returns<IntegrationEventHandlerContext, IntegrationEventHandlerDelegate>((context, next) =>
                next(context));

        var mockFilter2 = new Mock<IIntegrationEventHandlerFilter>();

        bool filter2Called = false;
        mockFilter2.Setup(p =>
                p.HandleAsync(It.IsAny<IntegrationEventHandlerContext>(),
                    It.IsAny<IntegrationEventHandlerDelegate>()))
            .Callback<IntegrationEventHandlerContext, IntegrationEventHandlerDelegate>((context, next) =>
            {
                Assert.True(filter1Called);
                filter2Called = true;
                Assert.NotNull(context);
                Assert.NotNull(context.Data);
                Assert.NotNull(context.Headers);
            }).Returns<IntegrationEventHandlerContext, IntegrationEventHandlerDelegate>((contxt, next) =>
                next(contxt));

        var wrap =
            new IntegrationEventHandlerWrap<IIntegrationEventHandler<TestIntegrationEvent>, TestIntegrationEvent>(
                handler: handlerMock.Object,
                new IIntegrationEventHandlerFilter[] { mockFilter1.Object, mockFilter2.Object });

        await wrap.HandleAsync(new TestIntegrationEvent("abc", 10),
            headers: new Dictionary<string, string?>(),
            new CancellationToken());

        Assert.True(handlerCalled);
        Assert.True(filter1Called);
    }

    public record TestIntegrationEvent(string Name, int Age);
}