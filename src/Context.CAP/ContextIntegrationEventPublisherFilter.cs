using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Context.CAP
{
    public class ContextIntegrationEventPublisherFilter : ContextProcessor, IIntegrationEventPublisherFilter
    {
        private readonly IContextAccessor _contextAccessor;

        public ContextIntegrationEventPublisherFilter(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
        }

        public Task PublishAsync(IntegrationEventPublishContext context, IntegrationEventPublishDelegate next)
        {
            var contextCarrier = new CapContextCarrier(context);
            InjectCarrier(_contextAccessor, contextCarrier);
            return next(context);
        }
    }
}