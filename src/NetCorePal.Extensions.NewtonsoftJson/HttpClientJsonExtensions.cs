using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public static class HttpClientJsonExtensions
{
    private static readonly Func<HttpClient, Uri?, CancellationToken, Task<HttpResponseMessage>> SDeleteAsync =
        ((client, uri, cancellation) =>
            client.DeleteAsync(uri, cancellation));

    private static readonly Func<HttpClient, Uri?, CancellationToken, Task<HttpResponseMessage>> SGetAsync =
        ((client, uri, cancellation) =>
            client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellation));


    private static Task<object?> FromNewtonsoftJsonAsyncCore(
        Func<HttpClient, Uri?, CancellationToken, Task<HttpResponseMessage>> getMethod,
        HttpClient client,
        Uri? requestUri,
        Type type,
        JsonSerializerSettings? options1,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore(getMethod, client,
            requestUri,
            (Func<HttpContent, (Type, JsonSerializerSettings?), CancellationToken, ValueTask<object?>>)(
                async (stream, options2, cancellation) => JsonConvert.DeserializeObject(
                    await stream.ReadAsStringAsync(cancellation), options2.Item1,
                    options2.Item2 ?? NewtonsoftJsonDefaults.DefaultOptions)), (type, options1),
            cancellationToken);
    }

    private static Task<TValue?> FromNewtonsoftJsonAsyncCore<TValue>(
        Func<HttpClient, Uri?, CancellationToken, Task<HttpResponseMessage>> getMethod,
        HttpClient client,
        Uri? requestUri,
        JsonSerializerSettings? options1,
        CancellationToken cancellationToken = default)
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore(getMethod, client, requestUri,
            (
                async (stream, options2, cancellation) => JsonConvert.DeserializeObject<TValue>(
                    await stream.ReadAsStringAsync(cancellation),
                    options2 ?? NewtonsoftJsonDefaults.DefaultOptions) ?? throw new NullReferenceException("序列化为空")),
            options1,
            cancellationToken);
    }


    private static Task<TValue?> FromNewtonsoftJsonAsyncCore<TValue, TJsonOptions>(
        Func<HttpClient, Uri?, CancellationToken, Task<HttpResponseMessage>> getMethod,
        HttpClient client,
        Uri? requestUri,
        Func<HttpContent, TJsonOptions?, CancellationToken, ValueTask<TValue?>> deserializeMethod,
        TJsonOptions? jsonOptions,
        CancellationToken cancellationToken)
    {
        TimeSpan delay = client.Timeout;
        CancellationTokenSource? linkedCTS = null;
        if (delay != Timeout.InfiniteTimeSpan)
        {
            linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCTS.CancelAfter(delay);
        }

        Task<HttpResponseMessage> responseTask;
        try
        {
            responseTask = getMethod(client, requestUri, cancellationToken);
        }
        catch
        {
            linkedCTS?.Dispose();
            throw;
        }

        return Core(responseTask, linkedCTS, deserializeMethod, jsonOptions,
            cancellationToken);

        static async Task<TValue?> Core(
            Task<HttpResponseMessage> responseTask,
            CancellationTokenSource? linkedCTS,
            Func<HttpContent, TJsonOptions?, CancellationToken, ValueTask<TValue?>> deserializeMethod,
            TJsonOptions? jsonOptions,
            CancellationToken cancellationToken)
        {
            TValue? obj;
            try
            {
                using (HttpResponseMessage response = await responseTask.ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    try
                    {
                        using (response.Content)
                        {
                            Func<HttpContent, TJsonOptions?, CancellationToken, ValueTask<TValue?>> func =
                                deserializeMethod;
                            HttpContent stream = response.Content;
                            TJsonOptions? jsonOptions1 = jsonOptions;
                            CancellationTokenSource? cancellationTokenSource = linkedCTS;
                            CancellationToken cancellationToken1 = cancellationTokenSource != null
                                ? cancellationTokenSource.Token
                                : cancellationToken;
                            obj = await func(stream, jsonOptions1, cancellationToken1).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
                    {
                        throw new TaskCanceledException(
                            "请求取消了",
                            new TimeoutException(ex.Message, ex), ex.CancellationToken);
                    }
                }
            }
            finally
            {
                linkedCTS?.Dispose();
            }

            return obj;
        }
    }

    private static Uri? CreateUri(string? uri) =>
        !string.IsNullOrEmpty(uri) ? new Uri(uri, UriKind.RelativeOrAbsolute) : null;

    private static HttpContent CreateContent<TValue>(TValue value, JsonSerializerSettings? options)
    {
        StringContent stringContent =
            new StringContent(JsonConvert.SerializeObject(value, options ?? NewtonsoftJsonDefaults.DefaultOptions));
        stringContent.Headers.ContentType = NewtonsoftJsonDefaults.JsonMediaType();
        return stringContent;
    }

    public static Task<object?> DeleteFromNewtonsoftJsonAsync(
        this HttpClient client,
        string? requestUri,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync(HttpClientJsonExtensions.CreateUri(requestUri), type, options,
            cancellationToken);
    }

    public static Task<object?> DeleteFromNewtonsoftJsonAsync(
        this HttpClient client,
        Uri? requestUri,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore(HttpClientJsonExtensions.SDeleteAsync, client,
            requestUri,
            type, options, cancellationToken);
    }

    public static Task<TValue?> DeleteFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync<TValue>(HttpClientJsonExtensions.CreateUri(requestUri), options,
            cancellationToken);
    }

    public static Task<TValue?> DeleteFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore<TValue>(HttpClientJsonExtensions.SDeleteAsync,
            client,
            requestUri, options, cancellationToken);
    }

    public static Task<object?> DeleteFromNewtonsoftJsonAsync(
        this HttpClient client,
        string? requestUri,
        Type type,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync(requestUri, type, null, cancellationToken);
    }

    public static Task<object?> DeleteFromNewtonsoftJsonAsync(
        this HttpClient client,
        Uri? requestUri,
        Type type,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync(requestUri, type, null, cancellationToken);
    }

    public static Task<TValue?> DeleteFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync<TValue>(requestUri, null, cancellationToken);
    }

    public static Task<TValue?> DeleteFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.DeleteFromNewtonsoftJsonAsync<TValue>(requestUri, null, cancellationToken);
    }

    public static Task<object?> GetFromNewtonsoftJsonAsync(
        this HttpClient client,
        string? requestUri,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync(HttpClientJsonExtensions.CreateUri(requestUri), type, options,
            cancellationToken);
    }

    public static Task<object?> GetFromNewtonsoftJsonAsync(
        this HttpClient client,
        Uri? requestUri,
        Type type,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore(
            ((client2, uri, cancellation) =>
                client2.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellation)), client, requestUri,
            type, options, cancellationToken);
    }

    public static Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync<TValue>(HttpClientJsonExtensions.CreateUri(requestUri), options,
            cancellationToken);
    }

    public static Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        JsonSerializerSettings? options,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return HttpClientJsonExtensions.FromNewtonsoftJsonAsyncCore<TValue>(HttpClientJsonExtensions.SGetAsync, client,
            requestUri, options, cancellationToken);
    }

    public static Task<object?> GetFromNewtonsoftJsonAsync(
        this HttpClient client,
        string? requestUri,
        Type type,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync(requestUri, type, null, cancellationToken);
    }

    public static Task<object?> GetFromNewtonsoftJsonAsync(
        this HttpClient client,
        Uri? requestUri,
        Type type,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync(requestUri, type, null, cancellationToken);
    }

    public static Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync<TValue>(requestUri, null, cancellationToken);
    }

    public static Task<TValue?> GetFromNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return client.GetFromNewtonsoftJsonAsync<TValue>(requestUri, null, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PostAsync(requestUri, content, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PostAsync(requestUri, content, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PostAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PostAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }


    public static Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PutAsync(requestUri, content, cancellationToken);
    }

    public static Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PutAsync(requestUri, content, cancellationToken);
    }


    public static Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PutAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }


    public static Task<HttpResponseMessage> PutAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PutAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PatchAsync(requestUri, content, cancellationToken);
    }


    public static Task<HttpResponseMessage> PatchAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        JsonSerializerSettings? options = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        var content = CreateContent(value, options: options);
        return client.PatchAsync(requestUri, content, cancellationToken);
    }


    public static Task<HttpResponseMessage> PatchAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        string? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PatchAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }


    public static Task<HttpResponseMessage> PatchAsNewtonsoftJsonAsync<TValue>(
        this HttpClient client,
        Uri? requestUri,
        TValue value,
        CancellationToken cancellationToken)
    {
        return client.PatchAsNewtonsoftJsonAsync(requestUri, value, null,
            cancellationToken);
    }
}