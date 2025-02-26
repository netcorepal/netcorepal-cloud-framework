using Microsoft.Extensions.Options;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv;

internal class NetCorePalServiceChecker(IServiceDiscoveryClient serviceDiscoveryClient)
    : IServiceChecker
{
    public ValueTask<bool> ServiceExist(string serviceName)
    {
        return new ValueTask<bool>(serviceDiscoveryClient.GetServiceClusters()
            .ContainsKey(serviceName));
    }

    public string GetEnvServiceName(string originalServiceName, string env)
    {
        return $"{originalServiceName}-{env}";
    }
}