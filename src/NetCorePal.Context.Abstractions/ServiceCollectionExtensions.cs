using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Context;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContextCore(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IContextAccessor>(ContextAccessor.Instance));
            services.TryAddSingleton(ContextAccessor.Instance);
            return services;
        }
    }
}
