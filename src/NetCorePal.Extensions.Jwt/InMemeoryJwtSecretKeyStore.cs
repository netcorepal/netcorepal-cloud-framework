namespace NetCorePal.Extensions.Jwt;

public class InMemeoryJwtSecretKeyStore : IJwtSecretKeyStore
{
    private static readonly IEnumerable<JwtSecretKeySetting> _secretKeySettings;

    static InMemeoryJwtSecretKeyStore()
    {
        _secretKeySettings = new[]
        {
            SecretKeyGenerator.GenerateRsaKeys()
        };
    }


    public Task< IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings()
    {
        return Task.FromResult( _secretKeySettings);
    }
}