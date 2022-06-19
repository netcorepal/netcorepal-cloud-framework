using System;
namespace NetCorePal.ServiceDiscovery
{
    public interface IServiceCluster
    {
        string ClusterId { get; }
        string? LoadBalancingPolicy { get; }

        IReadOnlyDictionary<string, IDestination> Destinations { get; }

        IReadOnlyDictionary<string, string>? Metadata { get;}
    }


    public record class ServiceCluster : IServiceCluster
    {
        public string ClusterId { get; init; } = default!;

        public string? LoadBalancingPolicy { get; init; } = default!;

        public IReadOnlyDictionary<string, IDestination> Destinations { get; init; } = default!;

        public IReadOnlyDictionary<string, string>? Metadata { get; init; }
    }                       
}
