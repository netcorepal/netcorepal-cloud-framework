using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.AspNetCore
{
    public static class ResponseDataExtensions
    {
        public static ResponseData<TData> AsResponseData<TData>(this TData data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
        {
            return new ResponseData<TData>(data: data, success: success, message: message, code: code, errorData: errorData);
        }
    }
}
