using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NetCorePal.Extensions.Jwt.UnitTests;

public class InMemoryJwtSettingStoreTests
{
    [Fact]
    public async Task GetSecretKeySettings_SaveSecretKeySettings_Test()
    {
        IServiceCollection sercices = new ServiceCollection();


        sercices.AddNetCorePalJwt().AddInMemoryStore();
        var provider = sercices.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        Assert.IsType<InMemoryJwtSettingStore>(store);

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
    }
}