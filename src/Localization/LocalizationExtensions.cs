using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace NetCorePal.Extensions.Localization;

/// <summary>
/// Configuration options for NetCorePal localization
/// </summary>
public class NetCorePalLocalizationOptions
{
    /// <summary>
    /// Default culture if not specified. Defaults to English (en)
    /// </summary>
    public string DefaultCulture { get; set; } = "en";

    /// <summary>
    /// Supported cultures. Defaults to English and Chinese.
    /// </summary>
    public string[] SupportedCultures { get; set; } = ["en", "zh"];

    /// <summary>
    /// Whether to fall back to default culture when a translation is not found
    /// </summary>
    public bool FallbackToDefaultCulture { get; set; } = true;
}

/// <summary>
/// Extensions for adding NetCorePal localization services
/// </summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds NetCorePal localization services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration for localization options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNetCorePalLocalization(
        this IServiceCollection services,
        Action<NetCorePalLocalizationOptions>? configure = null)
    {
        var options = new NetCorePalLocalizationOptions();
        configure?.Invoke(options);

        services.Configure<NetCorePalLocalizationOptions>(opt =>
        {
            opt.DefaultCulture = options.DefaultCulture;
            opt.SupportedCultures = options.SupportedCultures;
            opt.FallbackToDefaultCulture = options.FallbackToDefaultCulture;
        });

        // Add localization services
        services.AddLocalization();

        // Add framework's shared string localizer
        services.AddSingleton<IStringLocalizer<SharedResource>, StringLocalizer<SharedResource>>();

        // Add a factory for getting localizers for any type
        services.AddTransient<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();

        return services;
    }

    /// <summary>
    /// Sets the current thread culture based on the provided culture name
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "en", "zh", "en-US")</param>
    public static void SetCurrentCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}

/// <summary>
/// Helper class for accessing framework localization resources
/// </summary>
public static class FrameworkLocalizer
{
    private static IStringLocalizer<SharedResource>? _localizer;

    /// <summary>
    /// Initialize the framework localizer (should be called during app startup)
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve the localizer</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _localizer = serviceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();
    }

    /// <summary>
    /// Get a localized string by key
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="arguments">Optional arguments for string formatting</param>
    /// <returns>Localized string</returns>
    public static string GetString(string key, params object[] arguments)
    {
        if (_localizer == null)
        {
            throw new InvalidOperationException("FrameworkLocalizer has not been initialized. Call Initialize() during app startup.");
        }

        return _localizer[key, arguments];
    }

    /// <summary>
    /// Get a localized string by key
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <returns>Localized string</returns>
    public static string GetString(string key)
    {
        if (_localizer == null)
        {
            throw new InvalidOperationException("FrameworkLocalizer has not been initialized. Call Initialize() during app startup.");
        }

        return _localizer[key];
    }
}