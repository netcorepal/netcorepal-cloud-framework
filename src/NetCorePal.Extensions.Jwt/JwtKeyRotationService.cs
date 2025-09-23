using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Default implementation of JWT key rotation service
/// </summary>
public class JwtKeyRotationService : IJwtKeyRotationService
{
    private readonly IJwtSettingStore _store;
    private readonly IOptions<JwtKeyRotationOptions> _options;
    private readonly ILogger<JwtKeyRotationService> _logger;

    public JwtKeyRotationService(
        IJwtSettingStore store,
        IOptions<JwtKeyRotationOptions> options,
        ILogger<JwtKeyRotationService> logger)
    {
        _store = store;
        _options = options;
        _logger = logger;
    }

    public async Task<bool> IsRotationNeededAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Value.AutomaticRotationEnabled)
        {
            return false;
        }

        var settings = (await _store.GetSecretKeySettings(cancellationToken)).ToArray();
        var activeKeys = settings.Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTimeOffset.UtcNow)).ToArray();

        // Need rotation if no active keys or if the newest active key is close to expiration
        if (activeKeys.Length == 0)
        {
            _logger.LogInformation("Key rotation needed: No active keys found");
            return true;
        }

        var newestKey = activeKeys.OrderByDescending(k => k.CreatedAt).First();
        var timeUntilExpiration = newestKey.ExpiresAt?.Subtract(DateTimeOffset.UtcNow) ?? TimeSpan.MaxValue;
        
        // Rotate when the newest key has less than 25% of its lifetime remaining
        var rotationThreshold = _options.Value.KeyLifetime.Multiply(0.25);
        
        if (timeUntilExpiration < rotationThreshold)
        {
            _logger.LogInformation("Key rotation needed: Newest key expires in {TimeUntilExpiration}, threshold is {RotationThreshold}", 
                timeUntilExpiration, rotationThreshold);
            return true;
        }

        return false;
    }

    public async Task<bool> RotateKeysAsync(CancellationToken cancellationToken = default)
    {
        var settings = (await _store.GetSecretKeySettings(cancellationToken)).ToList();
        
        // Generate new key
        var newKey = SecretKeyGenerator.GenerateRsaKeys() with 
        { 
            ExpiresAt = DateTimeOffset.UtcNow.Add(_options.Value.KeyLifetime),
            IsActive = true
        };
        
        settings.Add(newKey);
        
        // Mark old keys as inactive if we exceed the max active keys limit
        var activeKeys = settings.Where(k => k.IsActive).OrderByDescending(k => k.CreatedAt).ToArray();
        if (activeKeys.Length > _options.Value.MaxActiveKeys)
        {
            var keysToDeactivate = activeKeys.Skip(_options.Value.MaxActiveKeys);
            foreach (var key in keysToDeactivate)
            {
                var index = settings.FindIndex(s => s.Kid == key.Kid);
                if (index >= 0)
                {
                    settings[index] = settings[index] with { IsActive = false };
                }
            }
        }

        await _store.SaveSecretKeySettings(settings, cancellationToken);
        
        _logger.LogInformation("JWT key rotation completed. New key ID: {KeyId}, Expires: {ExpiresAt}", 
            newKey.Kid, newKey.ExpiresAt);
        
        return true;
    }

    public async Task<int> CleanupExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        var settings = (await _store.GetSecretKeySettings(cancellationToken)).ToList();
        var cutoffTime = DateTimeOffset.UtcNow.Subtract(_options.Value.ExpiredKeyRetentionPeriod);
        
        var keysToRemove = settings.Where(k => 
            !k.IsActive && 
            k.ExpiresAt.HasValue && 
            k.ExpiresAt.Value < cutoffTime).ToArray();

        if (keysToRemove.Length == 0)
        {
            return 0;
        }

        var remainingKeys = settings.Except(keysToRemove).ToArray();
        await _store.SaveSecretKeySettings(remainingKeys, cancellationToken);
        
        _logger.LogInformation("Cleaned up {Count} expired JWT keys", keysToRemove.Length);
        
        return keysToRemove.Length;
    }
}