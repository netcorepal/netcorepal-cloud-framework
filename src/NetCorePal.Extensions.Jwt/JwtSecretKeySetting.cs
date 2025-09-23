namespace NetCorePal.Extensions.Jwt;

public record JwtSecretKeySetting(
    string PrivateKey,
    string Kid,
    string Kty,
    string Alg,
    string Use,
    string N,
    string E)
{
    /// <summary>
    /// When the key was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// When the key expires and should no longer be used for signing new tokens
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }
    
    /// <summary>
    /// Whether this key is currently active for signing new tokens
    /// </summary>
    public bool IsActive { get; init; } = true;
}