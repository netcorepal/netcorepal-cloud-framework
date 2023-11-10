namespace NetCorePal.Extensions.DistributedTransactions;

public sealed class IntegrationEventHandlerWrap<TIntegrationEventHandler, TIntegrationEvent>
    where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
{
    private readonly IntegrationEventHandlerDelegate _next;

    public IntegrationEventHandlerWrap(TIntegrationEventHandler handler,
        IEnumerable<IIntegrationEventHandlerFilter> filters)
    {
        IntegrationEventHandlerDelegate next = context =>
            handler.HandleAsync((TIntegrationEvent)context.Data, context.CancellationToken);
        foreach (var filter in filters.Reverse())
        {
            var current = next;
            next = context => filter.HandleAsync(context, current);
        }

        _next = next;
    }


    public Task HandleAsync(TIntegrationEvent eventData,
        IDictionary<string, string?>? headers = default,
        CancellationToken cancellationToken = default)
    {
        var context =
            new IntegrationEventHandlerContext<TIntegrationEvent>(eventData,
                headers ?? new Dictionary<string, string?>(),
                cancellationToken);
        return _next.Invoke(context);
    }
}