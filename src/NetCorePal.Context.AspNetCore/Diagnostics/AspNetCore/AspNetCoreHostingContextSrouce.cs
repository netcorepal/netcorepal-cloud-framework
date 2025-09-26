using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace NetCorePal.Context.Diagnostics.AspNetCore
{
    public class AspNetCoreHostingContextSrouce : IContextSource
    {
        private readonly HttpContext _httpContext;
        public AspNetCoreHostingContextSrouce(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public virtual string? Get(string key)
        {
            return TryGetCookie(key) ?? TryGetHeader(key);
        }

        protected virtual string? TryGetHeader(string key)
        {
            var header = _httpContext.Request.Headers.TryGetValue(key, out var val) ? val.ToString() : null;
            if (header != null)
            {
                try
                {
                    header = System.Web.HttpUtility.UrlDecode(header, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    var logger = _httpContext.RequestServices.GetService<ILogger<AspNetCoreHostingContextSrouce>>();
                    logger?.LogError(ex, "上下文UrlDecode失败：{header}", header);
                }
            }
            return header;
        }

        protected virtual string? TryGetCookie(string key)
        {
            var cookie = _httpContext.Request.Cookies.TryGetValue(key, out var val) ? val : null;
            return cookie;
        }
    }
}
