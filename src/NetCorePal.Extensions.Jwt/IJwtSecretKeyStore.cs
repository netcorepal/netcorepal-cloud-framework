namespace NetCorePal.Extensions.Jwt;

public interface IJwtSecretKeyStore
{
    public Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings();
}