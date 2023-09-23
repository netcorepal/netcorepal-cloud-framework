using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Context;
using NetCorePal.Context.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCapContextProcessor(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IContextProcessor, CapContextProcessor>());
            services.TryAddSingleton<CapContextProcessor>(p =>
                (p.GetRequiredService<IEnumerable<IContextProcessor>>().FirstOrDefault(cp => cp is CapContextProcessor)
                    as CapContextProcessor)!);
            services.TryAddSingleton<IPublisherFilter>(p => p.GetRequiredService<CapContextProcessor>());
            return services;
        }
    }
}