using System.Collections.Generic;

namespace NetCorePal.Extensions.Primitives
{
    /// <summary>
    /// 表示一个业务异常
    /// </summary>
    public interface IKnownException
    {
        string Message { get; }

        int ErrorCode { get; }

        IEnumerable<object> ErrorData { get; }
    }
}
