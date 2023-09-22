namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public class EventPublishContext<TEvent>
{
    public EventPublishContext(TEvent data, IDictionary<string, string?> headers)
    {
        Data = data;
        Headers = headers;
    }

    public TEvent Data { get; set; }
    public IDictionary<string, string?> Headers { get; set; }
}