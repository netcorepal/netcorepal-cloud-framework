using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Background service that periodically refreshes JWT keys from storage to detect changes from other nodes
/// </summary>
public class JwtKeyRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<JwtKeyRotationOptions> _options;
    private readonly ILogger<JwtKeyRefreshBackgroundService> _logger;

    public JwtKeyRefreshBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<JwtKeyRotationOptions> options,
        ILogger<JwtKeyRefreshBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Calculate refresh interval
        var refreshInterval = _options.Value.KeyRefreshInterval;
        if (refreshInterval == TimeSpan.Zero)
        {
            // Auto-calculate as 1/4 of the activation delay, with a minimum of 5 seconds and maximum of 30 seconds
            refreshInterval = TimeSpan.FromMilliseconds(Math.Max(5000, Math.Min(30000, _options.Value.NewKeyActivationDelay.TotalMilliseconds / 4)));
        }
        
        // If refresh interval is still zero or negative, disable the service
        if (refreshInterval <= TimeSpan.Zero)
        {
            _logger.LogInformation("JWT key refresh background service disabled (KeyRefreshInterval <= 0)");
            return;
        }
        
        _logger.LogInformation("JWT key refresh background service started with interval: {RefreshInterval}", refreshInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var optionsUpdater = scope.ServiceProvider.GetRequiredService<IJwtOptionsUpdater>();

                // Refresh JWT options with latest keys from storage
                await optionsUpdater.UpdateOptionsAsync(stoppingToken);
                
                _logger.LogDebug("JWT keys refreshed from storage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during JWT key refresh");
            }

            try
            {
                await Task.Delay(refreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("JWT key refresh background service stopped");
    }
}