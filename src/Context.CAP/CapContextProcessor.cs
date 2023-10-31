using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Context.CAP
{
    public class CapContextProcessor : ContextProcessor, IPublisherFilter, ISubscribeFilter
    {
        private readonly IContextAccessor _contextAccessor;

        private readonly ILogger<CapContextSrource> _loggerForCapContextSrouce;

        public CapContextProcessor(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSrource>();
        }

        public int Order => 0;

        public Task OnPublishAsync<TEvent>(EventPublishContext<TEvent> context,
            CancellationToken cancellationToken = default)
            where TEvent : notnull
        {
            var contextCarrier = new CapContextCarrier<TEvent>(context);
            InjectCarrier(_contextAccessor, contextCarrier);
            return Task.CompletedTask;
        }

        public Task OnSubscribeExecutingAsync(ExecutingContext context)
        {
            var contextSource = new CapContextSrource(context, _loggerForCapContextSrouce);
            ExtractSource(_contextAccessor, contextSource);
            return Task.CompletedTask;
        }

        public Task OnSubscribeExecutedAsync(ExecutedContext context)
        {
            return Task.CompletedTask;
        }

        public Task OnSubscribeExceptionAsync(ExceptionContext context)
        {
            return Task.CompletedTask;
        }
    }
}