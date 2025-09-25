using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace NetCorePal.Extensions.Jwt.UnitTests;

public class JwtKeyRotationServiceTests
{
    [Fact]
    public async Task IsRotationNeededAsync_NoKeys_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var rotationService = provider.GetRequiredService<IJwtKeyRotationService>();

        // Act
        var needsRotation = await rotationService.IsRotationNeededAsync();

        // Assert
        Assert.True(needsRotation);
    }

    [Fact]
    public async Task RotateKeysAsync_GeneratesNewKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        var rotationService = provider.GetRequiredService<IJwtKeyRotationService>();

        // Act
        var rotated = await rotationService.RotateKeysAsync();
        var keys = (await store.GetSecretKeySettings()).ToArray();

        // Assert
        Assert.True(rotated);
        Assert.Single(keys);
        Assert.True(keys[0].IsActive);
        Assert.NotNull(keys[0].ExpiresAt);
    }

    [Fact]
    public async Task RotateKeysAsync_LimitsActiveKeys()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        var rotationService = provider.GetRequiredService<IJwtKeyRotationService>();

        // Generate initial keys
        await rotationService.RotateKeysAsync();
        await rotationService.RotateKeysAsync();

        // Act - rotate again which should deactivate the oldest key
        await rotationService.RotateKeysAsync();
        var keys = (await store.GetSecretKeySettings()).ToArray();

        // Assert
        Assert.Equal(3, keys.Length); // 3 total keys
        Assert.Equal(2, keys.Count(k => k.IsActive)); // Only 2 active
        Assert.Single(keys.Where(k => !k.IsActive)); // 1 inactive
    }

    [Fact]
    public async Task CleanupExpiredKeysAsync_RemovesOldExpiredKeys()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt(options => options.ExpiredKeyRetentionPeriod = TimeSpan.FromDays(1))
            .AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        var rotationService = provider.GetRequiredService<IJwtKeyRotationService>();

        // Create an old expired key manually
        var oldKey = SecretKeyGenerator.GenerateRsaKeys() with
        {
            IsActive = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-2), // Expired 2 days ago
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var recentKey = SecretKeyGenerator.GenerateRsaKeys() with
        {
            IsActive = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };

        await store.SaveSecretKeySettings([oldKey, recentKey]);

        // Act
        var cleanedCount = await rotationService.CleanupExpiredKeysAsync();
        var remainingKeys = (await store.GetSecretKeySettings()).ToArray();

        // Assert
        Assert.Equal(1, cleanedCount);
        Assert.Single(remainingKeys);
        Assert.Equal(recentKey.Kid, remainingKeys[0].Kid);
    }

    [Fact]
    public async Task JwtProvider_UsesEarliestGeneratedUnexpiredKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        var jwtProvider = provider.GetRequiredService<IJwtProvider>();

        // Create a newer key
        var newerKey = SecretKeyGenerator.GenerateRsaKeys() with
        {
            IsActive = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow // Just created
        };

        // Create an older key that should be used (earliest generated)
        var olderKey = SecretKeyGenerator.GenerateRsaKeys() with
        {
            IsActive = true,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow.AddSeconds(-10) // Created 10 seconds ago
        };

        await store.SaveSecretKeySettings([newerKey, olderKey]);

        var jwtData = new JwtData(
            Issuer: "test-issuer",
            Audience: "test-audience",
            Claims: [new Claim("sub", "user123")],
            NotBefore: DateTime.UtcNow,
            Expires: DateTime.UtcNow.AddHours(1)
        );

        // Act - should use the older (earliest generated) key
        var token = await jwtProvider.GenerateJwtToken(jwtData);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // The token should be signed with the older (earliest generated) key
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        Assert.Equal(olderKey.Kid, jwtToken.Header.Kid);
    }
}