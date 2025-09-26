using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public static class HttpContentJsonExtensions
{
    public static Task<object?> ReadFromNewtonsoftJsonAsync(
        this HttpContent content,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));
        return HttpContentJsonExtensions.ReadFromNewtonsoftJsonAsyncCore(content, type, options, cancellationToken);
    }

    public static Task<object?> ReadFromNewtonsoftJsonAsync(
        this HttpContent content,
        Type type,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return content.ReadFromNewtonsoftJsonAsync(type, null, cancellationToken);
    }

    public static Task<T?> ReadFromNewtonsoftJsonAsync<T>(
        this HttpContent content,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));
        return HttpContentJsonExtensions.ReadFromNewtonsoftJsonAsyncCore<T>(content, options, cancellationToken);
    }

    public static Task<T?> ReadFromNewtonsoftJsonAsync<T>(
        this HttpContent content,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return content.ReadFromNewtonsoftJsonAsync<T>(null, cancellationToken);
    }

    private static async Task<object?> ReadFromNewtonsoftJsonAsyncCore(
        HttpContent content,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken)
    {
        var json = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var obj = JsonConvert.DeserializeObject(json, type, options ?? NewtonsoftJsonDefaults.DefaultOptions);
        return obj;
    }

    private static async Task<T?> ReadFromNewtonsoftJsonAsyncCore<T>(
        HttpContent content,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken)
    {
        var json = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var obj = JsonConvert.DeserializeObject<T>(json, options ?? NewtonsoftJsonDefaults.DefaultOptions);
        return obj;
    }
}