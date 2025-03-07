using Microsoft.EntityFrameworkCore;

namespace NetCorePal.Extensions.Jwt.EntityFrameworkCore;

public interface IJwtSettingDbContext
{
    public DbSet<JwtSettingData> JwtSettings { get; }
}