using DotNetCore.CAP;
using NetCorePal.Extensions.EventBus;

namespace NetCorePal.Extensions.EventBus.DotNetCoreCAP
{
    public class CAPEventPublisher : IEventPublisher
    {
        ICapPublisher _capPublisher;

        public CAPEventPublisher(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task PublishAsync<T>(string eventName, T eventData, CancellationToken cancellationToken = default) where T : class
        {
            return _capPublisher.PublishAsync<T>(eventName, eventData, callbackName: null, cancellationToken);
        }
    }



}