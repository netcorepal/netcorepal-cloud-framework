using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Jwt;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddInMemoryStore(this IJwtBuilder builder)
    {
        builder.Services.AddSingleton<IJwtSecretKeyStore, InMemeoryJwtSecretKeyStore>();
        return builder;
    }
}