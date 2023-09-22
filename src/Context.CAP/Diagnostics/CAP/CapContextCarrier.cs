using System.Net.Http;
using System.Text;
using NetCorePal.Context;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Context.Diagnostics.CAP
{
    public class CapContextCarrier<TEvent> : IContextCarrier
    {
        private readonly EventPublishContext<TEvent> _context;

        public CapContextCarrier(EventPublishContext<TEvent> context)
        {
            _context = context;
        }

        public void Set(string key, string val)
        {
            val = System.Web.HttpUtility.UrlEncode(val, Encoding.UTF8);
            _context.Headers.TryAdd(key, val);
        }
    }
}