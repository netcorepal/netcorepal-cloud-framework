using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace NetCorePal.Extensions.Jwt;

/// <summary>
/// Decorator that adds DataProtection encryption to JWT setting storage
/// </summary>
public class DataProtectionJwtSettingStore : IJwtSettingStore
{
    private readonly IJwtSettingStore _innerStore;
    private readonly IDataProtector _dataProtector;

    public DataProtectionJwtSettingStore(IJwtSettingStore innerStore, IDataProtectionProvider dataProtectionProvider)
    {
        _innerStore = innerStore;
        _dataProtector = dataProtectionProvider.CreateProtector("NetCorePal.Extensions.Jwt.Settings");
    }

    public async Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(CancellationToken cancellationToken = default)
    {
        var encryptedSettings = await _innerStore.GetSecretKeySettings(cancellationToken);
        var decryptedSettings = new List<JwtSecretKeySetting>();

        foreach (var setting in encryptedSettings)
        {
            try
            {
                // Decrypt the private key
                var decryptedPrivateKey = _dataProtector.Unprotect(setting.PrivateKey);
                var decryptedSetting = setting with { PrivateKey = decryptedPrivateKey };
                decryptedSettings.Add(decryptedSetting);
            }
            catch (Exception)
            {
                // If decryption fails, it might be an unencrypted key from before DataProtection was enabled
                // In this case, we'll treat it as already decrypted
                decryptedSettings.Add(setting);
            }
        }

        return decryptedSettings;
    }

    public async Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings, CancellationToken cancellationToken = default)
    {
        var encryptedSettings = settings.Select(setting =>
        {
            // Encrypt the private key
            var encryptedPrivateKey = _dataProtector.Protect(setting.PrivateKey);
            return setting with { PrivateKey = encryptedPrivateKey };
        });

        await _innerStore.SaveSecretKeySettings(encryptedSettings, cancellationToken);
    }
}