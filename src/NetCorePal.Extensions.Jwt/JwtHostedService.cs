using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NetCorePal.Extensions.Jwt;

public class JwtHostedService(
    IJwtSettingStore store,
    IOptionsMonitor<JwtBearerOptions> old,
    IPostConfigureOptions<JwtBearerOptions> options) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await UpdateJwtOptionsAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Periodically refresh JWT options every 30 seconds
        var refreshInterval = TimeSpan.FromSeconds(30);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(refreshInterval, stoppingToken);
                await UpdateJwtOptionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception)
            {
                // Continue on errors to avoid stopping the service
            }
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
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30), // Default 30 days
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