namespace NetCorePal.Extensions.Jwt;

public class InMemoryJwtSettingStore : IJwtSettingStore
{
    private IEnumerable<JwtSecretKeySetting> _secretKeySettings = [];

    public Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_secretKeySettings);
    }

    public Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings,
        CancellationToken cancellationToken = default)
    {
        _secretKeySettings = settings;
        return Task.CompletedTask;
    }
}