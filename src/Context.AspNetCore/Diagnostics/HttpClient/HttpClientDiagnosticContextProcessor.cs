using Microsoft.Extensions.DiagnosticAdapter;
using System.Diagnostics;
using System.Reactive;

namespace NetCorePal.Context.Diagnostics.HttpClient
{
    public class HttpClientDiagnosticContextProcessor : ContextProcessor
    {
        private readonly IContextAccessor _contextAccessor;
        public HttpClientDiagnosticContextProcessor(IContextAccessor contextAccessor, IEnumerable<IContextSourceHandler> sourceHandlers, IEnumerable<IContextCarrierHandler> carrierHandlers)
        {
            _contextAccessor = contextAccessor;
            SourceHandlers = sourceHandlers.ToList();
            CarrierHandlers = carrierHandlers.ToList();
            DiagnosticListener.AllListeners.Subscribe(Observer.Create<DiagnosticListener>(diagnostic => diagnostic.SubscribeWithAdapter(this)));
        }


        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest(HttpRequestMessage request)
        {
            var contextCarrier = new HttpClientContextCarrier(request);
            InjectCarrier(_contextAccessor, contextCarrier);
        }
    }
}
