using Microsoft.AspNetCore.Http;

namespace NetCorePal.Extensions.AspNetCore;

public interface IRequestCancellationToken
{
    CancellationToken CancellationToken { get; }
}

public class HttpContextAccessorRequestAbortedHandler : IRequestCancellationToken
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAccessorRequestAbortedHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CancellationToken CancellationToken =>
        _httpContextAccessor.HttpContext!.RequestAborted!;
}