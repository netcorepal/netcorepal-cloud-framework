using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Security.Claims;

namespace NetCorePal.Extensions.Jwt.UnitTests;

public class JwtIntegrationTests
{
    [Fact]
    public async Task FullWorkflow_KeyRotationAndTokenGeneration_Works()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt()
            .AddInMemoryStore()
            .UseKeyRotation(options =>
            {
                options.KeyLifetime = TimeSpan.FromMinutes(1); // Short lifetime for testing
                options.MaxActiveKeys = 2;
                options.AutomaticRotationEnabled = true;
            });
        services.AddLogging();
        
        var provider = services.BuildServiceProvider();
        
        // Start the hosted service to initialize keys
        var hostedService = provider.GetRequiredService<JwtHostedService>();
        await hostedService.StartAsync(CancellationToken.None);
        
        var jwtProvider = provider.GetRequiredService<IJwtProvider>();
        var rotationService = provider.GetRequiredService<IJwtKeyRotationService>();
        var store = provider.GetRequiredService<IJwtSettingStore>();

        // Act 1 - Generate initial token
        var jwtData = new JwtData(
            Issuer: "test-issuer",
            Audience: "test-audience", 
            Claims: [new Claim("sub", "user123")],
            NotBefore: DateTime.UtcNow,
            Expires: DateTime.UtcNow.AddHours(1)
        );
        
        var token1 = await jwtProvider.GenerateJwtToken(jwtData);
        Assert.NotNull(token1);
        Assert.NotEmpty(token1);

        // Act 2 - Rotate keys manually
        var rotated = await rotationService.RotateKeysAsync();
        Assert.True(rotated);

        // Verify we now have 2 keys
        var keys = (await store.GetSecretKeySettings()).ToArray();
        Assert.Equal(2, keys.Length);
        Assert.Equal(2, keys.Count(k => k.IsActive));

        // Act 3 - Generate token with new key
        var token2 = await jwtProvider.GenerateJwtToken(jwtData);
        Assert.NotNull(token2);
        Assert.NotEmpty(token2);
        Assert.NotEqual(token1, token2); // Should be different due to different key

        // Act 4 - Rotate again to test max keys limit
        await rotationService.RotateKeysAsync();
        
        keys = (await store.GetSecretKeySettings()).ToArray();
        Assert.Equal(3, keys.Length); // 3 total keys
        Assert.Equal(2, keys.Count(k => k.IsActive)); // Still only 2 active
        Assert.Single(keys.Where(k => !k.IsActive)); // 1 inactive

        // Cleanup
        await hostedService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task DataProtection_WithFileStore_WorksEndToEnd()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var services = new ServiceCollection();
            services.AddNetCorePalJwt()
                .AddFileStore(tempFile)
                .UseDataProtection();
            services.AddLogging();
            
            var provider = services.BuildServiceProvider();
            
            // Start the hosted service to initialize keys
            var hostedService = provider.GetRequiredService<JwtHostedService>();
            await hostedService.StartAsync(CancellationToken.None);
            
            var jwtProvider = provider.GetRequiredService<IJwtProvider>();

            // Act - Generate token
            var jwtData = new JwtData(
                Issuer: "test-issuer",
                Audience: "test-audience", 
                Claims: [new Claim("sub", "user123")],
                NotBefore: DateTime.UtcNow,
                Expires: DateTime.UtcNow.AddHours(1)
            );
            
            var token = await jwtProvider.GenerateJwtToken(jwtData);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            // Verify file contains encrypted data
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.Contains("\"PrivateKey\":", fileContent);
            
            // Cleanup
            await hostedService.StopAsync(CancellationToken.None);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}