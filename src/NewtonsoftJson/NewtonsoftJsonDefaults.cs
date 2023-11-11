using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public static class NewtonsoftJsonDefaults
{
    static NewtonsoftJsonDefaults()
    {
        var p = new JsonSerializerSettings();
        DefaultOptions = p;
    }

    public static JsonSerializerSettings DefaultOptions { get; }

    public static MediaTypeHeaderValue JsonMediaType() => new("application/json")
    {
        CharSet = "utf-8"
    };
}