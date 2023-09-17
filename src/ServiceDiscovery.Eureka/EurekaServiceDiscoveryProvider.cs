using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Steeltoe.Discovery.Eureka;
namespace NetCorePal.Extensions.ServiceDiscovery.Eureka;
public class EurekaServiceDiscoveryProvider : IServiceDiscoveryProvider
{
    readonly EurekaDiscoveryClient _eurekaClient;
    private CancellationTokenSource _cts = new();
    private IEnumerable<IServiceCluster> _clusters = new List<IServiceCluster>();
    private IChangeToken _token;

    public EurekaServiceDiscoveryProvider(IOptions<EurekaProviderOption> options, EurekaDiscoveryClient eurekaClient)
    {
        var eco = new EurekaClientOptions
        {
            ShouldOnDemandUpdateStatusChange = true,
            ServiceUrl = options.Value.ServerUrl,
            ShouldRegisterWithEureka = options.Value.RegisterService,
            ShouldFilterOnlyUpInstances = options.Value.OnlyUpInstances,
            ShouldGZipContent = options.Value.GZipContent,
            EurekaServerConnectTimeoutSeconds = options.Value.ConnectTimeoutSeconds,
            ValidateCertificates = options.Value.ValidateCertificates,
            RegistryFetchIntervalSeconds = options.Value.FetchIntervalSeconds
        };
        _eurekaClient = eurekaClient;
        _eurekaClient.OnApplicationsChange += EurekaClient_OnApplicationsChange;
        ReLoad();
        _token = new CancellationChangeToken(_cts.Token);
    }



    public IEnumerable<IServiceCluster> Clusters
    {
        get
        {
            return _clusters;
        }
    }



    void ReLoad()
    {
        List<IServiceCluster> clusters = new();

        foreach (var app in _eurekaClient.Applications.GetRegisteredApplications())
        {

            var cluster = new ServiceCluster
            {
                ClusterId = app.Name,
                Destinations = app.Instances.ToDictionary(p => p.InstanceId,
                       p => new Destination(p.AppName, p.InstanceId, p.IpAddr, p.Metadata) as IDestination)
            };
            clusters.Add(cluster);
        }
        _clusters = clusters;
    }



    public Task DeregisterAsync(CancellationToken cancellationToken = default)
    {
        return _eurekaClient.ShutdownAsync();
    }



    public IChangeToken GetReloadToken()
    {
        return _token;
    }


    private void EurekaClient_OnApplicationsChange(object? sender, Steeltoe.Discovery.Eureka.AppInfo.Applications e)
    {
        //TODO: 这里需要考虑并发情况 
        var _oldcts = _cts;
        _cts = new CancellationTokenSource();
        _token = new CancellationChangeToken(_cts.Token);
        ReLoad();
        _oldcts.Cancel();
    }

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
