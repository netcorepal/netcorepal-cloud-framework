using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Jwt.EntityFrameworkCore;

class DbContextJwtSettingStore<TDbContext>(IServiceProvider serviceProvider) : IJwtSettingStore
    where TDbContext : DbContext, IJwtSettingDbContext
{
    public async Task<IEnumerable<JwtSecretKeySetting>> GetSecretKeySettings(
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return (await dbContext.JwtSettings.OrderBy(p => p.Id).ToListAsync(cancellationToken))
            .Select(x => JsonSerializer.Deserialize<JwtSecretKeySetting>(x.JwtSetting)!).ToList();
    }

    public async Task SaveSecretKeySettings(IEnumerable<JwtSecretKeySetting> settings,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        dbContext.RemoveRange(await dbContext.JwtSettings.ToListAsync(cancellationToken));
        await dbContext.SaveChangesAsync(cancellationToken);
        await dbContext.JwtSettings.AddRangeAsync(
            settings.Select(x => new JwtSettingData { JwtSetting = JsonSerializer.Serialize(x) }), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}