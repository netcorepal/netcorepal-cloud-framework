using System;
using System.Text;
using DotNetCore.CAP.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCorePal.Context;

namespace NetCorePal.Context.Diagnostics.CAP
{
    public class CapContextSrouce : IContextSource
    {
        private readonly ExecutingContext _context;
        private ILogger _logger;

        public CapContextSrouce(ExecutingContext context, ILogger<CapContextSrouce> logger)
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