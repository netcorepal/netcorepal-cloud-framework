using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ABC.Extensions.EventBus
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
            ///TODO 这种获取eventHandler类型的方式可能造成内存泄露
            IEnumerable<IEventHandler> eventHandlers = _serviceProvider.GetServices<IEventHandler>();



            IEnumerable<Type> eventHandlerTypes = eventHandlers.Select(p => p.GetType());


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
