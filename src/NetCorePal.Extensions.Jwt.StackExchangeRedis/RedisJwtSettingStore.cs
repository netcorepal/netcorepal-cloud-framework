using StackExchange.Redis;
using System.Text.Json;

namespace NetCorePal.Extensions.Jwt.StackExchangeRedis;

public class RedisJwtSettingStore : IJwtSettingStore
{
    private readonly IDatabase _database;
    private readonly string _rediskey = "netcorepal:jwtsettings";

    public RedisJwtSettingStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }


    public async Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(_rediskey);
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        var settings = JsonSerializer.Deserialize<IEnumerable<JwtSecretKeySetting>>(value.ToString());
        if (settings == null)
        {
            return [];
        }

        return settings;
    }

    public async Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(settings);
        await _database.StringSetAsync(_rediskey, json);
    }
}