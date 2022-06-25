using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using NetCorePal.Context;

namespace NetCorePal.Context.Diagnostics.AspNetCore
{
    public class AspNetCoreHostingDiagnosticContextProcessor : ContextProcessor
    {
        private readonly IContextAccessor _contextAccessor;
        public AspNetCoreHostingDiagnosticContextProcessor(IContextAccessor contextAccessor, IEnumerable<IContextSourceHandler> sourceHandlers, IEnumerable<IContextCarrierHandler> carrierHandlers)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            DiagnosticListener.AllListeners.Subscribe(Observer.Create<DiagnosticListener>(diagnostic => diagnostic.SubscribeWithAdapter(this)));
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void BeginRequest(HttpContext httpContext)
        {
            var contextSource = new AspNetCoreHostingContextSrouce(httpContext);
            ExtractSource(_contextAccessor, contextSource);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void EndRequest(HttpContext httpContext)
        {
            ClearContext(_contextAccessor);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException(HttpContext httpContext, Exception exception)
        {
            ClearContext(_contextAccessor);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException(HttpContext httpContext, Exception exception)
        {
            ClearContext(_contextAccessor);
        }
    }
}
