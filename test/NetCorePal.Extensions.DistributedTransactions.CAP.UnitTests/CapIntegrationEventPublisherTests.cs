using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public class CapIntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishAsync()
    {
        bool icapPublisherCalled = false;
        var mockCapPublisher = new Mock<ICapPublisher>();
        mockCapPublisher.Setup(p =>
                p.PublishAsync(It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, object?, IDictionary<string, string?>, CancellationToken>(
                (name, contentObj, headers, cancellationToken) =>
                {
                    icapPublisherCalled = true;
                    Assert.Equal(nameof(TestIntegrationEvent), name);
                    Assert.NotNull(contentObj);
                    var obj = (TestIntegrationEvent)contentObj;
                    Assert.NotNull(obj);
                    Assert.Equal("abc", obj.Name);
                    Assert.Equal(10, obj.Age);
                })
            .Returns(Task.CompletedTask);
        var mockPublisherFilter1 = new Mock<IIntegrationEventPublisherFilter>();

        bool filter1Called = false;
        mockPublisherFilter1.Setup(p =>
                p.OnPublishAsync(It.IsAny<IntegrationEventPublishContext>(), It.IsAny<PublisherDelegate>()))
            .Callback<IntegrationEventPublishContext, PublisherDelegate>((context, next) =>
            {
                filter1Called = true;
                Assert.NotNull(context);
                Assert.NotNull(context.Data);
                Assert.NotNull(context.Headers);
            }).Returns<IntegrationEventPublishContext, PublisherDelegate>((context, next) =>
                next(context));

        var mockPublisherFilter2 = new Mock<IIntegrationEventPublisherFilter>();

        bool filter2Called = false;
        mockPublisherFilter2.Setup(p =>
                p.OnPublishAsync(It.IsAny<IntegrationEventPublishContext>(), It.IsAny<PublisherDelegate>()))
            .Callback<IntegrationEventPublishContext, PublisherDelegate>((context, next) =>
            {
                Assert.True(filter1Called);
                filter2Called = true;
                Assert.NotNull(context);
                Assert.NotNull(context.Data);
                Assert.NotNull(context.Headers);
            }).Returns<IntegrationEventPublishContext, PublisherDelegate>((contxt, next) =>
                next(contxt));

        CapIntegrationEventPublisher publisher =
            new CapIntegrationEventPublisher(capPublisher: mockCapPublisher.Object,
                new IIntegrationEventPublisherFilter[] { mockPublisherFilter1.Object, mockPublisherFilter2.Object });

        await publisher.PublishAsync(new TestIntegrationEvent("abc", 10), new CancellationToken());

        Assert.True(icapPublisherCalled);
        Assert.True(filter1Called);
    }

    public record TestIntegrationEvent(string Name, int Age);
}