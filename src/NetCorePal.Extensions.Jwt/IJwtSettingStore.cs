namespace NetCorePal.Extensions.Jwt;

public interface IJwtSettingStore
{
    public Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(CancellationToken cancellationToken = default);

    public Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings,
        CancellationToken cancellationToken = default);
}