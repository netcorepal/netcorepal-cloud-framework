using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Jwt;

public class FileJwtSettingStore : IJwtSettingStore
{
    private readonly FileJwtSettingStoreOptions _options;

    public FileJwtSettingStore(IOptions<FileJwtSettingStoreOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(CancellationToken cancellationToken = default)
    {
        var value = File.Exists(_options.FilePath) ? File.ReadAllText(_options.FilePath) : string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            return Task.FromResult<IEnumerable<JwtSecretKeySetting>>([]);
        }

        var settings = JsonSerializer.Deserialize<IEnumerable<JwtSecretKeySetting>>(value.ToString());
        if (settings == null)
        {
            return Task.FromResult<IEnumerable<JwtSecretKeySetting>>([]);
        }

        return Task.FromResult(settings);
    }

    public Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(_options.FilePath, json);
        return Task.CompletedTask;
    }
}

public class FileJwtSettingStoreOptions
{
    public string FilePath { get; set; } = string.Empty;
}