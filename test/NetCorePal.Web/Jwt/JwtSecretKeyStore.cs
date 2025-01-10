namespace NetCorePal.Web.Jwt;

public class JwtSecretKeyStore
{
    private static readonly IEnumerable<JwtSecretKeySetting> _secretKeySettings;

    static JwtSecretKeyStore()
    {
        _secretKeySettings = new[]
        {
            SecretKeyGenerator.GenerateRsaKeys()
        };
    }


    public IEnumerable<JwtSecretKeySetting> GetSecretKeySettings()
    {
        return _secretKeySettings;
    }
}