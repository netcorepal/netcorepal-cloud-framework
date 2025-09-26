using System.Text;

namespace NetCorePal.Context.Diagnostics.HttpClient
{
    public class HttpClientContextCarrier : IContextCarrier
    {
        private readonly HttpRequestMessage _request;
        public HttpClientContextCarrier(HttpRequestMessage request)
        {
            _request = request;
        }

        public void Set(string key, string val)
        {
            val = System.Web.HttpUtility.UrlEncode(val, Encoding.UTF8);
            _request.Headers.TryAddWithoutValidation(key, val);
        }
    }
}
