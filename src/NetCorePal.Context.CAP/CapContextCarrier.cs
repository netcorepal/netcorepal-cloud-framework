using System.Text;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Context.CAP
{
    public class CapContextCarrier : IContextCarrier
    {
        private readonly IntegrationEventPublishContext _context;

        public CapContextCarrier(IntegrationEventPublishContext context)
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