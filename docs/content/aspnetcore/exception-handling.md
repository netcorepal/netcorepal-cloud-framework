# 异常处理

## KnownException

我们在包`NetCorePal.Extensions.Primitives`中定义了一个异常类`KnownException`，用于表示已知异常，使得系统可以更友好的方式响应异常。

`KnownException`实现了接口 `IKnownException`，可以携带`Message`、`ErrorCode`、`ErrorData`等更多的信息，接口定义如下：

```csharp
public interface IKnownException
{
    string Message { get; }

    int ErrorCode { get; }

    IEnumerable<object> ErrorData { get; }
}
```

##  异常处理中间件 KnownExceptionHandleMiddleware

我们在包`NetCorePal.Extensions.AspNetCore`中定义了一个异常处理中间件`KnownExceptionHandleMiddleware`，用于处理异常。

`KnownExceptionHandleMiddleware`会捕获已知异常，并将异常信息转换为`ResponseData`对象，然后返回给客户端，从而保持正常响应与异常响应具有相同的数据结构，方便前端处理。

如果异常不是已知异常，则会返回一个默认的错误信息，以屏蔽敏感信息，避免暴露系统内部异常。

使用方法如下，在`Program.cs`文件中：

```csharp
var builder = WebApplication.CreateBuilder(args);

//Add Other Services

var app = builder.Build();
app.UseKnownExceptionHandler();
```

## KnownExceptionHandleMiddlewareOptions

`KnownExceptionHandleMiddleware`的配置选项如下：

+ `KnownExceptionStatusCode`: 已知异常响应状态码，默认为`HttpStatusCode.OK`，即`200`;
+ `UnknownExceptionStatusCode`: 非已知异常响应状态码，默认为`HttpStatusCode.InternalServerError`,即`500`;
+ `UnknownExceptionMessage`: 非已知异常响应消息，默认为`"未知错误"`;
+ `UnknownExceptionCode`: 非已知异常响应错误码，默认为`99999`;

具体代码定义如下：

```csharp
public class KnownExceptionHandleMiddlewareOptions
{
    public HttpStatusCode KnownExceptionStatusCode { get; set; } = HttpStatusCode.OK;
    public HttpStatusCode UnknownExceptionStatusCode { get; set; } = HttpStatusCode.InternalServerError;
    public string UnknownExceptionMessage { get; set; } = "未知错误";
    public int UnknownExceptionCode { get; set; } = 99999;
}
```

## 动态异常处理配置

`KnownExceptionHandleMiddleware`支持动态配置，通过注册一个工厂方法来实现：

```csharp
Func<HttpContext, KnownExceptionHandleMiddlewareOptions>
```

下面是一个示例，当请求路径以`/api/internal`开头时，使用`option1`，否则使用`option2`：

```csharp
var app = builder.Build();

var option1 = new KnownExceptionHandleMiddlewareOptions {
    KnownExceptionStatusCode = HttpStatusCode.InternalServerError,
    UnknownExceptionStatusCode = HttpStatusCode.InternalServerError,
    UnknownExceptionMessage = "未知错误",
    UnknownExceptionCode = 99999
};
var option2 = new KnownExceptionHandleMiddlewareOptions {
    KnownExceptionStatusCode = HttpStatusCode.OK,
    UnknownExceptionStatusCode = HttpStatusCode.BadRequest,
    UnknownExceptionMessage = "未知错误",
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
