using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventServicesBuilder
{
    IServiceCollection Services { get; }
}

class IntegrationEventServicesBuilder : IIntegrationEventServicesBuilder
{
    public IntegrationEventServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}