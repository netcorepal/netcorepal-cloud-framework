using System.Text;
using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Context.CAP
{
    public sealed class CapContextSource : IContextSource
    {
        private readonly IntegrationEventHandlerContext _context;
        private readonly ILogger _logger;

        public CapContextSource(IntegrationEventHandlerContext context, ILogger<CapContextSource> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string? Get(string key)
        {
            return TryGetHeader(key);
        }

        private string? TryGetHeader(string key)
        {
            var header = _context.Headers.TryGetValue(key, out var val) ? val : null;
            if (header != null)
            {
                try
                {
                    header = System.Web.HttpUtility.UrlDecode(header, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "上下文UrlDecode失败：{header}", header);
                }
            }

            return header;
        }
    }
}