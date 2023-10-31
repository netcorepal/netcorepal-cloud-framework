namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public interface IPublisherFilter
{
    int Order { get; }

    Task OnPublishAsync<TEvent>(EventPublishContext<TEvent> context, CancellationToken cancellationToken = default)
        where TEvent : notnull;
}

public class EmptyPublisherFilter : IPublisherFilter
{
    public int Order => 0;

    public Task OnPublishAsync<TEvent>(EventPublishContext<TEvent> context,
        CancellationToken cancellationToken = default) where TEvent : notnull
    {
        return Task.CompletedTask;
    }
}