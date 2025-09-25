using System.Security.Cryptography;
using NetCorePal.Extensions.DistributedLocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NetCorePal.Extensions.Jwt;

public class JwtHostedService(
    IJwtSettingStore store,
    IServiceProvider serviceProvider,
    IDistributedLock distributedLock,
    IOptions<JwtOptions> jwtOptions,
    IOptionsMonitor<JwtBearerOptions> old,
    IPostConfigureOptions<JwtBearerOptions> options,
    ILogger<JwtHostedService> logger) : BackgroundService
{
    const string LockKey = "NetCorePal:JwtHostedService:LockKey";

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await UpdateJwtOptionsAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!jwtOptions.Value.AutomaticRotationEnabled)
        {
            return;
        }

        TimeSpan refreshInterval = jwtOptions.Value.RotationCheckInterval;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(refreshInterval, stoppingToken);
                await using var handle = await distributedLock.AcquireAsync(LockKey, cancellationToken: stoppingToken);
                await HandleKeyRotationAsync(stoppingToken);
                await UpdateJwtOptionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred during JWT key management");
                // Continue on errors to avoid stopping the service
            }
        }
    }

    private async Task HandleKeyRotationAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Only handle rotation if key rotation service is available
            using var scope = serviceProvider.CreateScope();
            var rotationService = scope.ServiceProvider.GetService<IJwtKeyRotationService>();

            if (rotationService != null)
            {
                // Check if rotation is needed
                if (await rotationService.IsRotationNeededAsync(cancellationToken))
                {
                    logger.LogInformation("Performing automatic JWT key rotation");
                    await rotationService.RotateKeysAsync(cancellationToken);
                }

                // Clean up expired keys
                await rotationService.CleanupExpiredKeysAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during JWT key rotation");
        }
    }

    private async Task UpdateJwtOptionsAsync(CancellationToken cancellationToken)
    {
        var settings = (await store.GetSecretKeySettings(cancellationToken)).ToArray();
        if (!settings.Any())
        {
            // Generate initial key with expiration
            var initialKey = SecretKeyGenerator.GenerateRsaKeys() with
            {
                ExpiresAt = jwtOptions.Value.AutomaticRotationEnabled
                    ? DateTimeOffset.UtcNow.Add(jwtOptions.Value.KeyLifetime)
                    : DateTimeOffset.UtcNow.AddYears(100),
                IsActive = true
            };
            settings = [initialKey];
            await store.SaveSecretKeySettings(settings, cancellationToken);
        }


        var oldOptions = old.Get(JwtBearerDefaults.AuthenticationScheme);
        oldOptions.TokenValidationParameters ??= new TokenValidationParameters();

        // Include all keys (active and expired) for token validation
        // This allows validating tokens signed with expired keys
        oldOptions.TokenValidationParameters.IssuerSigningKeys = settings.Select(x =>
            new RsaSecurityKey(new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(x.E),
                Modulus = Base64UrlEncoder.DecodeBytes(x.N)
            })
            {
                KeyId = x.Kid
            });

        options.PostConfigure(JwtBearerDefaults.AuthenticationScheme, oldOptions);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}