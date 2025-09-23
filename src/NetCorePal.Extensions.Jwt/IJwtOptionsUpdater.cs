namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Service for updating JWT Bearer options when keys change
/// </summary>
public interface IJwtOptionsUpdater
{
    /// <summary>
    /// Updates the JWT Bearer options with current keys from storage
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateOptionsAsync(CancellationToken cancellationToken = default);
}