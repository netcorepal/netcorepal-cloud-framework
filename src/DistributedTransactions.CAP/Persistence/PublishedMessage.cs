namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class PublishedMessage
{
    public long Id { get; set; }
    public string? Version { get; set; }
    public string Name { get; set; } = null!;
    public string? Content { get; set; }
    public int? Retries { get; set; }
    public DateTime Added { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string StatusName { get; set; } = null!;
    
    public string DataSourceName { get; set; } = string.Empty;
}