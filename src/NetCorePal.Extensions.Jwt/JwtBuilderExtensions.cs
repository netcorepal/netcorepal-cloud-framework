using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddInMemoryStore(this IJwtBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore, InMemoryJwtSettingStore>());
        return builder;
    }

    public static IJwtBuilder AddFileStore(this IJwtBuilder builder, string filePath)
    {
        builder.Services.Configure<FileJwtSettingStoreOptions>(options => options.FilePath = filePath);
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore, FileJwtSettingStore>());
        return builder;
    }
}