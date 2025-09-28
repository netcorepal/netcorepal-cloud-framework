# 上下文传递系统

## 什么是上下文传递系统

上下文传递系统是一个用于传递上下文的系统，它可以在一个请求的生命周期中传递上下文，使得在一个请求的生命周期中，我们可以在任何地方访问到上下文。

## 如何注入上下文

在Program.cs中，我们可以通过下面代码注入上下文：

1. 注入上下文

    ```csharp
    builder.Services.AddContext()  // 添加上下文
        .AddEnvContext()    // 添加环境支持
        .AddTenantContext() // 添加租户支持
        .AddCapContextProcessor(); // 添加上下文在CAP中传递的支持，以支持集成事件处理器可以正确识别上下文
    ```

2. 添加上下文中间件

    ```csharp
    var app = builder.Build();
    
    app.UseContext();
    ```


## 如何使用上下文

我们可以在任何地方使用上下文，比如在Endpoint中：

```csharp
public class HomeEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/home");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 获取上下文
        var contextAccessor = Resolve<IContextAccessor>();
        var tenantContext = contextAccessor.GetContext<TenantContext>();

        // 设置上下文
        var tenantContext2 = new TenantContext("112233");
        contextAccessor.SetContext(tenantContext2);

        await SendOkAsync(ct);
    }
}
```

备注：你可以在一个服务内设置上下文，然后在另一个服务内获取上下文，上下文是在一个请求链路的生命周期内传递的，可以跨越服务进行传递。

## 默认提供的上下文类型
在包`NetCorePal.Context.Shared`中提供了一些默认的上下文类型，如：


1. 租户上下文

    类型：`NetCorePal.Context.TenantContext`，用以传递当前请求所属租户信息，以支持多租户系统的实现。


2. 环境上下文

    类型：`NetCorePal.Context.EnvContext`，用以传递当前请求的灰度环境信息，以支持灰度发布的实现。



## 支持的传递场景

默认情况下，上下文传递系统支持以下场景：

1. 支持在Http请求中传递上下文

    支持HttpClient对象发起请求时，自动传递上下文信息，同时支持aspnetcore接收请求时，自动解析上下文信息。

2. 支持在CAP中传递上下文

    支持CAP中发送事件时自动携带上下文信息，同时支持CAP中接收事件时自动解析上下文信息。