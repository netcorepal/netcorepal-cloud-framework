using System;
using System.Collections.Generic;

namespace ABC.Extensions.Primitives
{
    /// <summary>
    /// 表示一个业务异常
    /// </summary>
    public class KnownException : Exception, IKnownException
    {
        public KnownException(string message) : base(message) { }

        public KnownException(string message, int errorCode) : base(message) => ErrorCode = errorCode;
        public KnownException(string message, int errorCode, Exception innerException)
           : base(message, innerException) => ErrorCode = errorCode;

        public KnownException(string message, int errorCode, object[] errorData) : base(message)
        {
            ErrorCode = errorCode;
            ErrorData = errorData;
        }


        /// <summary>
        /// 每个领域都是单独的错误码集合
        /// </summary>
        public int ErrorCode { get; private set; }

        public IEnumerable<object> ErrorData { get; private set; } = new List<object>();


        internal const int UnknownErrorCode = 1000000;

        /// <summary>
        /// 表示一个未知的错误
        /// </summary>
        public static readonly IKnownException Unknown = new KnownException(message: "未知错误", errorCode: UnknownErrorCode);

        public KnownException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
