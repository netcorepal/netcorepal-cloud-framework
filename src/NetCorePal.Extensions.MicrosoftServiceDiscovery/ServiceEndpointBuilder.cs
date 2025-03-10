using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MicrosoftServiceDiscovery;

/// <summary>
/// Copy from <see cref="ServiceEndpointBuilder"/> in Microsoft.Extensions.ServiceDiscovery.
/// </summary>
internal sealed class ServiceEndpointBuilder : IServiceEndpointBuilder
{
    private readonly List<ServiceEndpoint> _endpoints = new();
    private readonly List<IChangeToken> _changeTokens = new();
    private readonly FeatureCollection _features = new FeatureCollection();

    /// <summary>
    /// Adds a change token.
    /// </summary>
    /// <param name="changeToken">The change token.</param>
    public void AddChangeToken(IChangeToken changeToken)
    {
        _changeTokens.Add(changeToken);
    }

    /// <summary>
    /// Gets the feature collection.
    /// </summary>
    public IFeatureCollection Features => _features;

    /// <summary>
    /// Gets the endpoints.
    /// </summary>
    public IList<ServiceEndpoint> Endpoints => _endpoints;

    /// <summary>
    /// Creates a <see cref="ServiceEndpointSource"/> from the provided instance.
    /// </summary>
    /// <returns>The service endpoint source.</returns>
    public ServiceEndpointSource Build()
    {
        return new ServiceEndpointSource(_endpoints, new CompositeChangeToken(_changeTokens), _features);
    }
}