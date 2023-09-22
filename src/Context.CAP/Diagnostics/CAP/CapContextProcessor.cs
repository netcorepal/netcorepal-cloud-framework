using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using DotNetCore.CAP;
using DotNetCore.CAP.Filter;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using NetCorePal.Context;
using NetCorePal.Context.Diagnostics.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Context.Diagnostics.HttpClient
{
    public class CapContextProcessor : ContextProcessor, IPublisherFilter, ISubscribeFilter
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CapContextSrouce> _loggerForCapContextSrouce;

        public CapContextProcessor(IContextAccessor contextAccessor,
            IEnumerable<IContextSourceHandler> sourceHandlers,
            IEnumerable<IContextCarrierHandler> carrierHandlers, ILoggerFactory loggerFactory)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            _loggerFactory = loggerFactory;
            _loggerForCapContextSrouce = loggerFactory.CreateLogger<CapContextSrouce>();
        }

        public int Order => 0;

        public Task OnPublishAsync<TEvent>(EventPublishContext<TEvent> context) where TEvent : notnull
        {
            var contextCarrier = new CapContextCarrier<TEvent>(context);
            InjectCarrier(_contextAccessor, contextCarrier);
            return Task.CompletedTask;
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