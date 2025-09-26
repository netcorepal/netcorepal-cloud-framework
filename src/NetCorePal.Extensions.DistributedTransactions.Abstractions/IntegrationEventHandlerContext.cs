namespace NetCorePal.Extensions.DistributedTransactions;

public class IntegrationEventHandlerContext
{
    public IntegrationEventHandlerContext(object data, IDictionary<string, string?> headers,
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

public class IntegrationEventHandlerContext<TEvent> : IntegrationEventHandlerContext where TEvent : notnull
{
    public IntegrationEventHandlerContext(TEvent data, IDictionary<string, string?> headers,
        CancellationToken cancellationToken = default) : base(data, headers, cancellationToken)
    {
        Data = data;
    }

    public new TEvent Data { get; set; }
}