using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddInMemoryStore(this IJwtBuilder builder)
    {
        builder.Services.AddSingleton<IJwtSettingStore, InMemoryJwtSettingStore>();
        return builder;
    }
}