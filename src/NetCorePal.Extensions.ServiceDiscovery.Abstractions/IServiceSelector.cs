namespace NetCorePal.Extensions.ServiceDiscovery;

public interface IServiceSelector
{
    IDestination? Find(string serviceName);
}