using System.ComponentModel.DataAnnotations.Schema;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class ReceivedMessage
{
    public long Id { get; set; }
    public string? Version { get; set; }
    public string Name { get; set; } = null!;
    public string? Group { get; set; }
    public string? Content { get; set; }
    public int? Retries { get; set; }
    public DateTime Added { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string StatusName { get; set; } = null!;
    
    [NotMapped]
    public string TenantId { get; set; } = string.Empty;
}