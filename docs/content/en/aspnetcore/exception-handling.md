# Exception Handling

## KnownException

We have defined an exception class `KnownException` in the package `NetCorePal.Extensions.Primitives` to represent known exceptions, allowing the system to respond to exceptions in a more user-friendly manner.

`KnownException` implements the `IKnownException` interface, which can carry more information such as `Message`, `ErrorCode`, `ErrorData`, etc. The interface is defined as follows:

```csharp
public interface IKnownException
{
    string Message { get; }

    int ErrorCode { get; }

    IEnumerable<object> ErrorData { get; }
}
```

## Exception Handling Middleware KnownExceptionHandleMiddleware

We have defined an exception handling middleware `KnownExceptionHandleMiddleware` in the package `NetCorePal.Extensions.AspNetCore` to handle exceptions.

`KnownExceptionHandleMiddleware` will catch known exceptions and convert the exception information into a `ResponseData` object, then return it to the client, ensuring that normal responses and exception responses have the same data structure, making it easier for the frontend to handle.

If the exception is not a known exception, a default error message will be returned to mask sensitive information and avoid exposing internal system exceptions.

Usage example in the `Program.cs` file:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Other Services

var app = builder.Build();
app.UseKnownExceptionHandler();
```

## KnownExceptionHandleMiddlewareOptions

The configuration options for `KnownExceptionHandleMiddleware` are as follows:

+ `KnownExceptionStatusCode`: The response status code for known exceptions, default is `HttpStatusCode.OK` (200);
+ `UnknownExceptionStatusCode`: The response status code for unknown exceptions, default is `HttpStatusCode.InternalServerError` (500);
+ `UnknownExceptionMessage`: The response message for unknown exceptions, default is `"Unknown Error"`;
+ `UnknownExceptionCode`: The response error code for unknown exceptions, default is `99999`;

The specific code definition is as follows:

```csharp
public class KnownExceptionHandleMiddlewareOptions
{
    public HttpStatusCode KnownExceptionStatusCode { get; set; } = HttpStatusCode.OK;
    public HttpStatusCode UnknownExceptionStatusCode { get; set; } = HttpStatusCode.InternalServerError;
    public string UnknownExceptionMessage { get; set; } = "Unknown Error";
    public int UnknownExceptionCode { get; set; } = 99999;
}
```

## Dynamic Exception Handling Configuration

`KnownExceptionHandleMiddleware` supports dynamic configuration through registering a factory method:

```csharp
Func<HttpContext, KnownExceptionHandleMiddlewareOptions>
```

Here is an example where `option1` is used when the request path starts with `/api/internal`, otherwise `option2` is used:

```csharp
var app = builder.Build();

var option1 = new KnownExceptionHandleMiddlewareOptions {
    KnownExceptionStatusCode = HttpStatusCode.InternalServerError,
    UnknownExceptionStatusCode = HttpStatusCode.InternalServerError,
    UnknownExceptionMessage = "Unknown Error",
    UnknownExceptionCode = 99999
};
var option2 = new KnownExceptionHandleMiddlewareOptions {
    KnownExceptionStatusCode = HttpStatusCode.OK,
    UnknownExceptionStatusCode = HttpStatusCode.BadRequest,
    UnknownExceptionMessage = "Unknown Error",
    UnknownExceptionCode = 10000
};

app.UseKnownExceptionHandler(httpContext => {
    if(httpContext.Request.Path.StartsWithSegments("/api/internal")) {
        return option1;
    }
    else {
        return option2;
    }
});
```
