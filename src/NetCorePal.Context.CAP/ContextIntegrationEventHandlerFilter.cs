using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Context.CAP
{
    public class ContextIntegrationEventHandlerFilter : ContextProcessor, IIntegrationEventHandlerFilter
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly ILogger<CapContextSource> _loggerForCapContextSrouce;

        public ContextIntegrationEventHandlerFilter(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSource>();
        }

        public Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
        {
            var contextSource = new CapContextSource(context, _loggerForCapContextSrouce);
            ExtractSource(_contextAccessor, contextSource);
            return next(context);
        }
    }
}