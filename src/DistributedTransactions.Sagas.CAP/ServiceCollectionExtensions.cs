using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.DistributedTransactions.Sagas.CAP;

// ReSharper disable once CheckNamespace
namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册SagaEventPublisher
        /// </summary>
        public static IServiceCollection AddCAPSagaEventPublisher(this IServiceCollection services)
        {
            services.TryAddSingleton<ISagaEventPublisher, CAPSagaEventPublisher>();
            return services;
        }
    }
}