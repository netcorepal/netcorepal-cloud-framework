using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.AspNetCore
{
    public class KnownExceptionHandleMiddlewareOptions
    {
        public HttpStatusCode KnownExceptionStatusCode { get; set; } = HttpStatusCode.OK;
        public HttpStatusCode UnknownExceptionStatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public string UnknownExceptionMessage { get; set; } = "未知错误";
        public int UnknownExceptionCode { get; set; } = 99999;
    }
}
