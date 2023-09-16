using NetCorePal.Context;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv;

public class EnvServiceSelector : IServiceSelector
{
    private readonly IContextAccessor _contextAccessor;
    private readonly IServiceDiscoveryClient _serviceDiscoveryClient;

    public EnvServiceSelector(IContextAccessor contextAccessor, IServiceDiscoveryClient serviceDiscoveryClient)
    {
        _contextAccessor = contextAccessor;
        _serviceDiscoveryClient = serviceDiscoveryClient;
    }

    public IDestination? Find(string serviceName)
    {
        var env = _contextAccessor.GetContext<EnvContext>()?.Evn;
        if (!string.IsNullOrEmpty(env))
        {
            var destination = FindByName($"{serviceName}-{env}");
            if (destination != null)
            {
                return destination;
            }
        }

        return FindByName(serviceName);
    }

    IDestination? FindByName(string serviceName)
    {
        var clusters = _serviceDiscoveryClient.GetServiceClusters();
        if (clusters.TryGetValue(serviceName, out var cluster))
        {
            return cluster.Destinations.FirstOrDefault().Value;
        }

        return null;
    }
}