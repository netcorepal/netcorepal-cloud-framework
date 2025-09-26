namespace NetCorePal.Extensions.ServiceDiscovery;

public class DefaultServiceSelector : IServiceSelector
{
    private readonly IServiceDiscoveryClient _serviceDiscoveryClient;

    public DefaultServiceSelector(IServiceDiscoveryClient serviceDiscoveryClient)
    {
        _serviceDiscoveryClient = serviceDiscoveryClient;
    }

    public IDestination? Find(string serviceName)
    {
        var clusters = _serviceDiscoveryClient.GetServiceClusters();
        if (clusters.TryGetValue(serviceName, out var cluster))
        {
            return cluster.Destinations.FirstOrDefault().Value;
        }

        return null;
    }
}