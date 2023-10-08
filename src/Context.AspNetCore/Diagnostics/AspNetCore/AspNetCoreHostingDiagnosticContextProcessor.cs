using System.Diagnostics;
using System.Reactive;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;

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
#pragma warning disable IDE0060 // 删除未使用的参数
        public void EndRequest(HttpContext httpContext)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            ClearContext(_contextAccessor);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
#pragma warning disable IDE0060 // 删除未使用的参数
        public void DiagnosticUnhandledException(HttpContext httpContext, Exception exception)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            ClearContext(_contextAccessor);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
#pragma warning disable IDE0060 // 删除未使用的参数
        public void HostingUnhandledException(HttpContext httpContext, Exception exception)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            ClearContext(_contextAccessor);
        }
    }
}
