using MassTransit;

namespace NetCorePal.Extensions.EventBus.MassTransit
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        IPublishEndpoint _publishEndpoint;
        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }


        public Task PublishAsync<T>(string eventName, T eventData, CancellationToken cancellationToken = default) where T : class
        {
            return _publishEndpoint.Publish(eventData, cancellationToken);
        }
    }
}