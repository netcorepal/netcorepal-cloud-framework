using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.AspNetCore
{
    public class ResponseData
    {
        public ResponseData(bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
        {
            Success = success;
            Message = message;
            Code = code;
            ErrorData = errorData ?? KnownException.EmptyErrorData;
        }

        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public int Code { get; protected set; }

        public IEnumerable<object> ErrorData { get; protected set; }
    }


    public class ResponseData<T> : ResponseData
    {
        public ResponseData(T data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null) : base(success: success, message: message, code: code, errorData: errorData)
        {
            this.Data = data;
        }
        public T Data { get; protected set; }
    }
}
