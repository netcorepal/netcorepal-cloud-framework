using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public interface ICapBuilder
{
    public IServiceCollection Services { get; }
}

internal class CapBuilder : ICapBuilder
{
    public CapBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}