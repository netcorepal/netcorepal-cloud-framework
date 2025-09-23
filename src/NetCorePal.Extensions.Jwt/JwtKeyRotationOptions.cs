namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Configuration options for JWT key rotation
/// </summary>
public class JwtKeyRotationOptions
{
    /// <summary>
    /// How long a key should be valid for signing new tokens (default: 30 days)
    /// </summary>
    public TimeSpan KeyLifetime { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// How often to check for key rotation (default: 1 hour)
    /// </summary>
    public TimeSpan RotationCheckInterval { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// How long to keep expired keys for validation of existing tokens (default: 7 days)
    /// </summary>
    public TimeSpan ExpiredKeyRetentionPeriod { get; set; } = TimeSpan.FromDays(7);
    
    /// <summary>
    /// Maximum number of active keys to maintain (default: 2)
    /// </summary>
    public int MaxActiveKeys { get; set; } = 2;
    
    /// <summary>
    /// Whether automatic key rotation is enabled (default: true)
    /// </summary>
    public bool AutomaticRotationEnabled { get; set; } = true;
}