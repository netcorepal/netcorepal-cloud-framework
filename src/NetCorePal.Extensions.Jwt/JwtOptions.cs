namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Configuration options for JWT key rotation
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Lifetime for a signing key when automatic rotation is enabled.
    /// Only takes effect if <see cref="AutomaticRotationEnabled"/> is true. When automatic rotation is disabled,
    /// newly generated keys are given a long lifetime (100 years) and this value is ignored.
    /// Default (when enabled): 30 days.
    /// </summary>
    public TimeSpan KeyLifetime { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// How often to check for key rotation (default: 1 hour)
    /// </summary>
    public TimeSpan RotationCheckInterval { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// How long to keep expired keys for validation of existing tokens (default: 30 days)
    /// </summary>
    public TimeSpan ExpiredKeyRetentionPeriod { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// Maximum number of active keys to maintain (default: 2)
    /// </summary>
    public int MaxActiveKeys { get; set; } = 2;
    
    /// <summary>
    /// Whether automatic key rotation is enabled (default: false)
    /// </summary>
    public bool AutomaticRotationEnabled { get; set; } = false;
}