using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Jwt.EntityFrameworkCore.UnitTests;

public class DbContextJwtSettingStoreTests
{
    [Fact]
    public async Task GetSecretKeySettings_SaveSecretKeySettings_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<JwtDbContext>(options => { options.UseInMemoryDatabase("test"); });
        services.AddNetCorePalJwt().AddEntityFrameworkCoreStore<JwtDbContext>();
        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<IJwtSettingStore>();
        Assert.IsType<DbContextJwtSettingStore<JwtDbContext>>(store);

        var list = (await store.GetSecretKeySettings()).ToList();
        Assert.NotNull(list);
        Assert.Empty(list);

        var secretKeySettings = new[]
        {
            new JwtSecretKeySetting("key1", "kid1", "kty1", "alg1", "use1", "n1", "e1"),
            new JwtSecretKeySetting("key2", "kid2", "kty2", "alg2", "use2", "n2", "e2")
        };
        await store.SaveSecretKeySettings(secretKeySettings);

        var newSettings = (await store.GetSecretKeySettings()).ToArray();
        Assert.NotNull(newSettings);
        Assert.NotEmpty(newSettings);
        Assert.Equal(2, newSettings.Length);
        Assert.Equal(secretKeySettings, newSettings);

        await store.SaveSecretKeySettings(secretKeySettings);

        var newSettings2 = (await store.GetSecretKeySettings()).ToArray();
        Assert.NotNull(newSettings2);
        Assert.NotEmpty(newSettings2);
        Assert.Equal(2, newSettings2.Length);
        Assert.Equal(secretKeySettings, newSettings2);
    }
}

public class JwtDbContext(DbContextOptions<JwtDbContext> options) : DbContext(options), IJwtSettingDbContext
{
    public DbSet<JwtSettingData> JwtSettings { get; set; }
}