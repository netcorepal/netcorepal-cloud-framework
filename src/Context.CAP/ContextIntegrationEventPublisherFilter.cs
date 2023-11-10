using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Context.CAP
{
    public class ContextIntegrationEventPublisherFilter : ContextProcessor, IIntegrationEventPublisherFilter
    {
        private readonly IContextAccessor _contextAccessor;

        private readonly ILogger<CapContextSource> _loggerForCapContextSrouce;

        public ContextIntegrationEventPublisherFilter(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSource>();
        }

        public Task OnPublishAsync(IntegrationEventPublishContext context, PublisherDelegate next)
        {
            var contextCarrier = new CapContextCarrier(context);
            InjectCarrier(_contextAccessor, contextCarrier);
            return next(context);
        }
    }
}