using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MicrosoftServiceDiscovery;

/// <summary>
/// Creates service endpoint watchers. Copy of the original class from the Microsoft.Extensions.ServiceDiscovery package.
/// </summary>
internal sealed partial class ServiceEndpointWatcherFactory(
    IEnumerable<IServiceEndpointProviderFactory> providerFactories,
    ILogger<ServiceEndpointWatcher> logger,
    IOptions<ServiceDiscoveryOptions> options,
    TimeProvider timeProvider)
{
    private readonly IServiceEndpointProviderFactory[] _providerFactories = providerFactories
        .Where(r => r.GetType().Name != "PassThroughServiceEndpointProviderFactory")
        //.Concat(providerFactories.Where(static r => r.GetType().Name == "PassThroughServiceEndpointProviderFactory"))
        .ToArray();

    /// <summary>
    /// Creates a service endpoint watcher for the provided service name.
    /// </summary>
    public ServiceEndpointWatcher CreateWatcher(string serviceName)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        if (!ServiceEndpointQuery.TryParse(serviceName, out var query))
        {
            throw new ArgumentException("The provided input was not in a valid format. It must be a valid URI.", nameof(serviceName));
        }

        List<IServiceEndpointProvider>? providers = null;
        foreach (var factory in _providerFactories)
        {
            if (factory.TryCreateProvider(query, out var provider))
            {
                providers ??= [];
                providers.Add(provider);
            }
        }

        if (providers is not { Count: > 0 })
        {
            throw new InvalidOperationException($"No provider which supports the provided service name, '{serviceName}', has been configured.");
        }

        return new ServiceEndpointWatcher(
            providers: [.. providers],
            logger: logger,
            serviceName: serviceName,
            timeProvider: timeProvider,
            options: options);
    }
}
