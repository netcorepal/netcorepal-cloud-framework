using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class SagaTimeoutException : KnownException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public SagaTimeoutException(string message) : base(message)
        {
        }
    }
}
