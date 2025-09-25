namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Service for managing JWT key rotation
/// </summary>
public interface IJwtKeyRotationService
{
    /// <summary>
    /// Manually trigger key rotation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if rotation occurred, false if no rotation was needed</returns>
    Task<bool> RotateKeysAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if key rotation is needed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if rotation is needed</returns>
    Task<bool> IsRotationNeededAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clean up expired keys that are no longer needed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of keys cleaned up</returns>
    Task<int> CleanupExpiredKeysAsync(CancellationToken cancellationToken = default);
}