using System.Text;
using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Context.CAP
{
    public class CapContextSrource : IContextSource
    {
        private readonly ExecutingContext _context;
        private ILogger _logger;

        public CapContextSrource(ExecutingContext context, ILogger<CapContextSrource> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual string? Get(string key)
        {
            return TryGetHeader(key);
        }

        protected virtual string? TryGetHeader(string key)
        {
            var header = _context.DeliverMessage.Headers.TryGetValue(key, out var val) ? val : null;
            if (header != null)
            {
                try
                {
                    header = System.Web.HttpUtility.UrlDecode(header, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "上下文UrlDecode失败：{0}", header);
                }
            }

            return header;
        }
    }
}