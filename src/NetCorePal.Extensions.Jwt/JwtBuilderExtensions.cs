using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Jwt;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddInMemoryStore(this IJwtBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<InMemoryJwtSettingStore>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore>(provider => provider.GetRequiredService<InMemoryJwtSettingStore>()));
        return builder;
    }

    public static IJwtBuilder AddFileStore(this IJwtBuilder builder, string filePath)
    {
        builder.Services.Configure<FileJwtSettingStoreOptions>(options => options.FilePath = filePath);
        builder.Services.Replace(ServiceDescriptor.Singleton<FileJwtSettingStore>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore>(provider => provider.GetRequiredService<FileJwtSettingStore>()));
        return builder;
    }
    
    /// <summary>
    /// Enables DataProtection encryption for JWT settings storage
    /// </summary>
    /// <param name="builder">The JWT builder</param>
    /// <returns>The JWT builder for chaining</returns>
    public static IJwtBuilder UseDataProtection(this IJwtBuilder builder)
    {
        // Ensure DataProtection is available
        builder.Services.AddDataProtection();
        
        // Replace the existing IJwtSettingStore with a decorated version
        var existingDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IJwtSettingStore));
        if (existingDescriptor != null)
        {
            builder.Services.Remove(existingDescriptor);
            
            // Register the decorated version
            builder.Services.Add(ServiceDescriptor.Describe(
                typeof(IJwtSettingStore),
                serviceProvider =>
                {
                    // Create the inner store
                    var innerStore = existingDescriptor.ImplementationType != null
                        ? (IJwtSettingStore)ActivatorUtilities.CreateInstance(serviceProvider, existingDescriptor.ImplementationType)
                        : existingDescriptor.ImplementationFactory!(serviceProvider) as IJwtSettingStore
                        ?? existingDescriptor.ImplementationInstance as IJwtSettingStore;

                    if (innerStore == null)
                    {
                        throw new InvalidOperationException("Could not resolve inner IJwtSettingStore for DataProtection decoration");
                    }

                    var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
                    return new DataProtectionJwtSettingStore(innerStore, dataProtectionProvider);
                },
                existingDescriptor.Lifetime));
        }
        
        return builder;
    }
    
    /// <summary>
    /// Enables JWT key rotation functionality
    /// </summary>
    /// <param name="builder">The JWT builder</param>
    /// <returns>The JWT builder for chaining</returns>
    public static IJwtBuilder UseKeyRotation(this IJwtBuilder builder)
    {
        // Configure default rotation options
        builder.Services.Configure<JwtKeyRotationOptions>(_ => { });
        
        // Add key rotation services
        builder.Services.AddSingleton<IJwtKeyRotationService, JwtKeyRotationService>();
        builder.Services.AddHostedService<JwtKeyRotationBackgroundService>();
        
        return builder;
    }
    
    /// <summary>
    /// Enables JWT key rotation functionality with custom configuration
    /// </summary>
    /// <param name="builder">The JWT builder</param>
    /// <param name="configureRotation">Action to configure rotation options</param>
    /// <returns>The JWT builder for chaining</returns>
    public static IJwtBuilder UseKeyRotation(this IJwtBuilder builder, Action<JwtKeyRotationOptions> configureRotation)
    {
        builder.Services.Configure(configureRotation);
        return builder.UseKeyRotation();
    }
}