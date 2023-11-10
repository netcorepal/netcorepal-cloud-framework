namespace NetCorePal.Extensions.DistributedTransactions;

public class IntegrationEventPublishContext
{
    public IntegrationEventPublishContext(object data, IDictionary<string, string?> headers,
        CancellationToken cancellationToken = default)
    {
        Data = data;
        Headers = headers;
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }

    public object Data { get; set; }
    public IDictionary<string, string?> Headers { get; set; }
}

public class IntegrationEventPublishContext<TEvent> : IntegrationEventPublishContext where TEvent : notnull
{
    public IntegrationEventPublishContext(TEvent data, IDictionary<string, string?> headers,
        CancellationToken cancellationToken = default) : base(data, headers, cancellationToken)
    {
        Data = data;
    }

    public new TEvent Data { get; set; }
}