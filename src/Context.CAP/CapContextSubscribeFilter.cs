using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Context.CAP
{
    public class CapContextSubscribeFilter : ContextProcessor, ISubscribeFilter
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly ILogger<CapContextSrource> _loggerForCapContextSrouce;

        public CapContextSubscribeFilter(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSrource>();
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