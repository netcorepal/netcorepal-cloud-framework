using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public class CapIntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishAsync()
    {
        var mockCapPublisher = new Mock<ICapPublisher>();
        mockCapPublisher.Setup(p =>
                p.PublishAsync("name",
                    It.IsAny<TestIntegrationEvent>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<CancellationToken>()))
            .Callback<string, TestIntegrationEvent?, IDictionary<string, string?>, CancellationToken>(
                (name, contentObj, headers, cancellationToken) =>
                {
                    Assert.Equal(nameof(TestIntegrationEvent), name);
                    Assert.NotNull(contentObj);
                    Assert.Equal("abc", contentObj.Name);
                    Assert.Equal(123, contentObj.Age);
                })
            .Returns(Task.CompletedTask);
        var mockPublisherFilters = new Mock<IPublisherFilter>();

        mockPublisherFilters.Setup(p =>
                p.OnPublishAsync(It.IsAny<EventPublishContext<TestIntegrationEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<EventPublishContext<TestIntegrationEvent>, CancellationToken>((context, cancellationToken) =>
            {
                Assert.NotNull(context);
                Assert.NotNull(context.Data);
                Assert.NotNull(context.Headers);
            });
        CapIntegrationEventPublisher publisher =
            new CapIntegrationEventPublisher(capPublisher: mockCapPublisher.Object,
                new[] { mockPublisherFilters.Object });

        await publisher.PublishAsync(new TestIntegrationEvent("abc", 10));
    }

    public record TestIntegrationEvent(string Name, int Age);
}