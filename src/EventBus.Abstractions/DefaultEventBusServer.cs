using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetCorePal.Extensions.EventBus
{
    public abstract class DefaultEventBusServer : IEventBusServer, IHostedService
    {

        IServiceProvider _serviceProvider;
        IConsumerClient _consumerClient;

        public DefaultEventBusServer(IServiceProvider serviceProvider, IConsumerClient consumerClient)
        {
            _serviceProvider = serviceProvider;
            _consumerClient = consumerClient;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Type> eventHandlerTypes = NetCorePal.Extensions.DependencyInjection.ServiceCollectionExtensions.handlerTypes;
            foreach (var eventHandlerType in eventHandlerTypes)
            {
                //TODO 添加订阅者相关配置逻辑
                await _consumerClient.SubscribeAsync("", "", data =>
                {
                    using (AsyncServiceScope scope = _serviceProvider.CreateAsyncScope())
                    {
                        //TODO 添加EventHandler<T>的调用逻辑
                        var eventHandler = scope.ServiceProvider.GetService(eventHandlerType);
                    }
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
