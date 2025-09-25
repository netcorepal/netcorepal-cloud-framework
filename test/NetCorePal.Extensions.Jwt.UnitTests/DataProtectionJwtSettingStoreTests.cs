using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace NetCorePal.Extensions.Jwt.UnitTests;

public class DataProtectionJwtSettingStoreTests
{
    [Fact]
    public async Task SaveAndRetrieve_WithDataProtection_EncryptsAndDecrypts()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore().UseDataProtection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();

        var originalKey = SecretKeyGenerator.GenerateRsaKeys();
        var originalPrivateKey = originalKey.PrivateKey;

        // Act
        await store.SaveSecretKeySettings([originalKey]);
        var retrievedKeys = (await store.GetSecretKeySettings()).ToArray();

        // Assert
        Assert.Single(retrievedKeys);
        var retrievedKey = retrievedKeys[0];
        
        // The retrieved private key should be the same as the original (decrypted)
        Assert.Equal(originalPrivateKey, retrievedKey.PrivateKey);
        Assert.Equal(originalKey.Kid, retrievedKey.Kid);
        Assert.Equal(originalKey.N, retrievedKey.N);
        Assert.Equal(originalKey.E, retrievedKey.E);
    }

    [Fact]
    public async Task SaveAndRetrieve_WithDataProtection_HandlesUnencryptedKeys()
    {
        // Arrange - first save without data protection
        var services = new ServiceCollection();
        services.AddNetCorePalJwt().AddInMemoryStore();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        var originalKey = SecretKeyGenerator.GenerateRsaKeys();
        await store.SaveSecretKeySettings([originalKey]);

        // Act - then retrieve with data protection (simulating upgrade scenario)
        var protectedServices = new ServiceCollection();
        protectedServices.AddNetCorePalJwt().AddInMemoryStore().UseDataProtection();
        protectedServices.AddLogging();
        var protectedProvider = protectedServices.BuildServiceProvider();

        // Manually create the DataProtection store with the existing data
        var innerStore = protectedProvider.GetRequiredService<InMemoryJwtSettingStore>();
        await innerStore.SaveSecretKeySettings([originalKey]); // Set the unencrypted data

        var dataProtectionProvider = protectedProvider.GetRequiredService<IDataProtectionProvider>();
        var protectedStore = new DataProtectionJwtSettingStore(innerStore, dataProtectionProvider);

        var retrievedKeys = (await protectedStore.GetSecretKeySettings()).ToArray();

        // Assert - should handle unencrypted keys gracefully
        Assert.Single(retrievedKeys);
        Assert.Equal(originalKey.PrivateKey, retrievedKeys[0].PrivateKey);
    }

    [Fact]
    public async Task FileStore_WithDataProtection_PersistsEncryptedData()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var services = new ServiceCollection();
            services.AddNetCorePalJwt().AddFileStore(tempFile).UseDataProtection();
            services.AddLogging();
            var provider = services.BuildServiceProvider();

            var store = provider.GetRequiredService<IJwtSettingStore>();
            var originalKey = SecretKeyGenerator.GenerateRsaKeys();

            // Act
            await store.SaveSecretKeySettings([originalKey]);

            // Assert - the file should contain encrypted data
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.Contains("\"PrivateKey\":", fileContent);
            // The encrypted private key should be different from the original
            Assert.DoesNotContain(originalKey.PrivateKey, fileContent);

            // Verify we can still retrieve the original data
            var retrievedKeys = (await store.GetSecretKeySettings()).ToArray();
            Assert.Single(retrievedKeys);
            Assert.Equal(originalKey.PrivateKey, retrievedKeys[0].PrivateKey);
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