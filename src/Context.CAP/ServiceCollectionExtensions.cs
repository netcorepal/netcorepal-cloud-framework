using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Context;
using NetCorePal.Context.CAP;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCapContextProcessor(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IContextProcessor, ContextIntegrationEventPublisherFilter>());
            services.TryAddSingleton<ContextIntegrationEventPublisherFilter>(p =>
                (p.GetRequiredService<IEnumerable<IContextProcessor>>()
                        .FirstOrDefault(cp => cp is ContextIntegrationEventPublisherFilter)
                    as ContextIntegrationEventPublisherFilter)!);
            services.TryAddSingleton<IIntegrationEventPublisherFilter>(p =>
                p.GetRequiredService<ContextIntegrationEventPublisherFilter>());
            return services;
        }

        public static ICapBuilder AddContextIntegrationFilters(
            this ICapBuilder builder)
        {
            builder.Services.AddSingleton<IIntegrationEventPublisherFilter, ContextIntegrationEventPublisherFilter>();
            builder.Services.AddSingleton<IIntegrationEventHandlerFilter, ContextIntegrationEventHandlerFilter>();
            return builder;
        }
    }
}