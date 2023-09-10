using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaTimeoutException : KnownException
    {
        public SagaTimeoutException(string message) : base(message)
        {
        }
    }
}
