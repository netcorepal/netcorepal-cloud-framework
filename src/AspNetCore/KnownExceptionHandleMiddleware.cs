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
        private readonly RequestDelegate _next;
        private readonly KnownExceptionHandleMiddlewareOptions _options;
        private readonly ILogger _logger;

        public KnownExceptionHandleMiddleware(RequestDelegate next, KnownExceptionHandleMiddlewareOptions options,
            ILogger<KnownExceptionHandleMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>()!;
            ResponseData responseData;
            if (exceptionHandlerFeature.Error is IKnownException ex)
            {
                context.Response.StatusCode = (int)_options.KnownExceptionStatusCode;
                responseData = new ResponseData(success: false, message: ex.Message, code: ex.ErrorCode,
                    errorData: ex.ErrorData);
            }
            else
            {
                _logger.LogError(exceptionHandlerFeature.Error, _options.UnknownExceptionMessage);
                context.Response.StatusCode = (int)_options.UnknownExceptionStatusCode;
                responseData = new ResponseData(success: false, message: _options.UnknownExceptionMessage,
                    code: _options.UnknownExceptionCode);
            }

            await context.Response.WriteAsJsonAsync(responseData, context.RequestAborted);
        }
    }
}