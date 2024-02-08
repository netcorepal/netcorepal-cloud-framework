using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Primitives;
using System.Globalization;

namespace NetCorePal.Extensions.AspNetCore
{
    public class KnownExceptionHandleMiddleware
    {
        private readonly ILogger _logger;
        private readonly Func<HttpContext, KnownExceptionHandleMiddlewareOptions> _optionFactory;

#pragma warning disable IDE0060 // 删除未使用的参数
        public KnownExceptionHandleMiddleware(RequestDelegate next, KnownExceptionHandleMiddlewareOptions options,
#pragma warning restore IDE0060 // 删除未使用的参数
            ILogger<KnownExceptionHandleMiddleware> logger)
        {
            _logger = logger;
            _optionFactory = _ => options;
        }

        public KnownExceptionHandleMiddleware(RequestDelegate next,
            Func<HttpContext, KnownExceptionHandleMiddlewareOptions> optionFactory,
            ILogger<KnownExceptionHandleMiddleware> logger)
        {
            _optionFactory = optionFactory;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            var options = _optionFactory(context);
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>()!;
            ResponseData responseData;
            if (exceptionHandlerFeature.Error is IKnownException ex)
            {
                context.Response.StatusCode = (int)options.KnownExceptionStatusCode;
                responseData = new ResponseData(success: false, message: ex.Message, code: ex.ErrorCode,
                    errorData: ex.ErrorData);
            }
            else
            {
                _logger.LogError(exceptionHandlerFeature.Error, message: "{message}", options.UnknownExceptionMessage);
                context.Response.StatusCode = (int)options.UnknownExceptionStatusCode;
                responseData = new ResponseData(success: false, message: options.UnknownExceptionMessage,
                    code: options.UnknownExceptionCode);
            }

            await context.Response.WriteAsJsonAsync(responseData, context.RequestAborted);
        }
    }
}