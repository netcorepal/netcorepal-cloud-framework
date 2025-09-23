using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace NetCorePal.Extensions.Localization.AspNetCore;

/// <summary>
/// ASP.NET Core extensions for NetCorePal localization
/// </summary>
public static class AspNetCoreLocalizationExtensions
{
    /// <summary>
    /// Adds NetCorePal localization with ASP.NET Core integration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddNetCorePalLocalizationWithAspNetCore(
        this IServiceCollection services,
        Action<NetCorePalLocalizationOptions>? configure = null)
    {
        services.AddNetCorePalLocalization(configure);

        // Configure request localization options
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var localizationOptions = new NetCorePalLocalizationOptions();
            configure?.Invoke(localizationOptions);

            var supportedCultures = localizationOptions.SupportedCultures
                .Select(c => new CultureInfo(c))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(localizationOptions.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            // Add request culture providers in order of priority
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
            options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });

        return services;
    }

    /// <summary>
    /// Use NetCorePal localization middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseNetCorePalLocalization(this IApplicationBuilder app)
    {
        // Initialize the framework localizer
        FrameworkLocalizer.Initialize(app.ApplicationServices);

        // Use request localization middleware
        app.UseRequestLocalization();

        return app;
    }
}

/// <summary>
/// Custom request culture provider that checks for culture in various sources
/// </summary>
public class CustomRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        // Check if culture is specified in headers (for API clients)
        if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
        {
            var culture = acceptLanguage.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(culture))
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
            }
        }

        // Check for culture in query string (?culture=zh)
        if (httpContext.Request.Query.TryGetValue("culture", out var queryCulture))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(queryCulture.ToString()));
        }

        // Check for culture in route values
        if (httpContext.Request.RouteValues.TryGetValue("culture", out var routeCulture))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(routeCulture?.ToString()));
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }
}