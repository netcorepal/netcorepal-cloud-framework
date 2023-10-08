using NetCorePal.Extensions.Primitives;

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
