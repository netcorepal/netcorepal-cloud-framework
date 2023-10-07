using System.Runtime.Serialization;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    /// <summary>
    /// 
    /// </summary>
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class RecursionOverflowException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public int MaxDeep { get; set; }

        public RecursionOverflowException(int maxDeep, string error) : base(error)
        {
            MaxDeep = maxDeep;
        }
    }
}
