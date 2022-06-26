using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Steeltoe.Discovery.Eureka;
namespace NetCorePal.ServiceDiscovery.Eureka;
public class EurekaServiceDiscoveryProvider : IServiceDiscoveryProvider
{
    DiscoveryClient _eurekaClient;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private IEnumerable<IServiceCluster>? _clusters;
    private IChangeToken _token;

    public EurekaServiceDiscoveryProvider(IOptions<EurekaProviderOption> options)
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
        _eurekaClient = new DiscoveryClient(eco);
        _eurekaClient.OnApplicationsChange += _eurekaClient_OnApplicationsChange;
        _token = new CancellationChangeToken(_cts.Token);
    }



    public IEnumerable<IServiceCluster> Clusters
    {
        get
        {
            if (_clusters == null)
            {
                _clusters = LoadAsyns();
            }
            return _clusters;
        }
    }



    List<IServiceCluster> LoadAsyns()
    {
        List<IServiceCluster> clusters = new List<IServiceCluster>();

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

        return clusters;
    }



    public Task DeregisterAsync(CancellationToken cancellationToken = default)
    {
        return _eurekaClient.ShutdownAsync();
    }



    public IChangeToken GetReloadToken()
    {
        return _token;
    }


    private void _eurekaClient_OnApplicationsChange(object? sender, Steeltoe.Discovery.Eureka.AppInfo.Applications e)
    {
        //TODO: 这里需要考虑并发情况 
        var _oldcts = _cts;
        _cts = new CancellationTokenSource();
        _token = new CancellationChangeToken(_cts.Token);
        _clusters = LoadAsyns();
        _oldcts.Cancel();
    }

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
