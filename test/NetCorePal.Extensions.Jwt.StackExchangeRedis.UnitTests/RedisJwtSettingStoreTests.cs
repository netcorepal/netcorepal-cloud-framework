using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using IServiceProvider = System.IServiceProvider;

namespace NetCorePal.Extensions.Jwt.StackExchangeRedis.UnitTests;

public class RedisJwtSettingStoreTests
{
    [Fact]
    public async Task GetSecretKeySettings_SaveSecretKeySettings_Test()
    {
        IServiceCollection sercices = new ServiceCollection();
        var mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        var mockDatabase = new Mock<IDatabase>();

        string? value = null;
        mockDatabase.Setup(p => p.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(() => value);
        mockDatabase.Setup(p =>
                p.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                    It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>((key, v, expiry, b, when, f) =>
                value = v);
        mockConnectionMultiplexer.Setup(p => p.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(mockDatabase.Object);

        sercices.AddSingleton<IConnectionMultiplexer>(mockConnectionMultiplexer.Object);
        sercices.AddNetCorePalJwt().AddRedisStore();
        var provider = sercices.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        Assert.IsType<RedisJwtSettingStore>(store);

        var settrings = await store.GetSecretKeySettings();
        Assert.Empty(settrings);

        IEnumerable<JwtSecretKeySetting> secretKeySettings =
        [
            new JwtSecretKeySetting(PrivateKey: "key1", Kid: "kid1", Kty: "kty1", Alg: "alg1", Use: "use1", N: "n1",
                E: "e1"),
            new JwtSecretKeySetting(PrivateKey: "key2", Kid: "kid2", Kty: "kty2", Alg: "alg2", Use: "use2", N: "n2",
                E: "e2")
        ];
        await store.SaveSecretKeySettings(secretKeySettings);

        var newSettings = (await store.GetSecretKeySettings()).ToArray();
        Assert.NotNull(newSettings);
        Assert.NotEmpty(newSettings);
        Assert.Equal(2, newSettings.Length);
        
        await store.SaveSecretKeySettings(secretKeySettings);

        var newSettings2 = (await store.GetSecretKeySettings()).ToArray();
        Assert.NotNull(newSettings2);
        Assert.NotEmpty(newSettings2);
        Assert.Equal(2, newSettings2.Length);
    }
}