using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.DistributedLocks.Redis;
using StackExchange.Redis;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisLocks(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedDisLock>(p =>
                new RedisLock(p.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));
            return services;
        }

        public static IServiceCollection AddRedisLocks(this IServiceCollection services,
            IConnectionMultiplexer connectionMultiplexer)
        {
            services.AddSingleton<IDistributedDisLock>(p =>
                new RedisLock(connectionMultiplexer.GetDatabase()));
            return services;
        }
    }
}