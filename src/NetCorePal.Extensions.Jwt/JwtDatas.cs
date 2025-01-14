namespace NetCorePal.Extensions.Jwt;

public static class JwtDatas
{
    internal static IEnumerable<JwtSecretKeySetting>? _secretKeySettings = null;
    
    public static IEnumerable<JwtSecretKeySetting> GetSecretKeySettings()
    {
        if (_secretKeySettings == null)
        {
            throw new Exception("Secret key settings are not initialized.");
        }

        return _secretKeySettings;
    }
}