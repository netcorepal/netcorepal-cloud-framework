using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.HttpClients;

public class KnownExceptionDelegatingHandler(ILogger<KnownExceptionDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage message;
        try
        {
            message = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "调用服务出错：{url}, HttpRequestException:{message}",
                request.RequestUri,
                e.Message);
            throw new KnownException("未知错误");
        }

        if (!message.IsSuccessStatusCode)
        {
            var data = await message.Content.ReadFromJsonAsync<ResponseData>(cancellationToken: cancellationToken);
            if (data != null)
            {
                throw new KnownException(data.Message, data.Code, data.ErrorData.ToArray());
            }
            else
            {
                throw new KnownException(message.ReasonPhrase ?? "未知错误");
            }
        }

        return message;
    }
}