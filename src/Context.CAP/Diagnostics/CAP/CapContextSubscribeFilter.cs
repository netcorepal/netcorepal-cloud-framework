using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Context.Diagnostics.CAP
{
    public class CapContextSubscribeFilter : ContextProcessor, ISubscribeFilter
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CapContextSrouce> _loggerForCapContextSrouce;

        public CapContextSubscribeFilter(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerFactory = loggerFactory;
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSrouce>();
        }
        
        public Task OnSubscribeExecutingAsync(ExecutingContext context)
        {
            var contextSource = new CapContextSrouce(context, _loggerForCapContextSrouce);
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