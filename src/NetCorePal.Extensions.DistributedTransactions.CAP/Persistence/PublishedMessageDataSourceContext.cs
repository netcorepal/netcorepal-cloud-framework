namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class PublishedMessageDataSourceContext
{
    private static readonly AsyncLocal<PublishedMessageDatabaseHolder> Current = new();

    public string GetDataSourceName()
    {
        return Current.Value?.DataSourceName ?? string.Empty;
    }

    public void SetDataSourceName(string databaseName)
    {
        if (Current.Value != null)
        {
            Current.Value.DataSourceName = databaseName;
        }
    }

    public void Init()
    {
        Current.Value = new PublishedMessageDatabaseHolder();
    }

    public void Clear()
    {
        Current.Value = null!;
    }
}

class PublishedMessageDatabaseHolder
{
    public string DataSourceName { get; internal set; } = string.Empty;
}