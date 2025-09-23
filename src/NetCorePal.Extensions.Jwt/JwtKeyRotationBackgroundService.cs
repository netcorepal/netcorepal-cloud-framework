using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Background service that handles automatic JWT key rotation
/// </summary>
public class JwtKeyRotationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<JwtKeyRotationOptions> _options;
    private readonly ILogger<JwtKeyRotationBackgroundService> _logger;

    public JwtKeyRotationBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<JwtKeyRotationOptions> options,
        ILogger<JwtKeyRotationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.AutomaticRotationEnabled)
        {
            _logger.LogInformation("Automatic JWT key rotation is disabled");
            return;
        }

        _logger.LogInformation("JWT key rotation background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var rotationService = scope.ServiceProvider.GetRequiredService<IJwtKeyRotationService>();

                // Check if rotation is needed
                if (await rotationService.IsRotationNeededAsync(stoppingToken))
                {
                    await rotationService.RotateKeysAsync(stoppingToken);
                }

                // Clean up expired keys
                await rotationService.CleanupExpiredKeysAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during JWT key rotation check");
            }

            try
            {
                await Task.Delay(_options.Value.RotationCheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("JWT key rotation background service stopped");
    }
}