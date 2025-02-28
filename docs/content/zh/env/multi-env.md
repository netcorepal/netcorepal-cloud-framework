# 多环境支持

## 什么是多环境支持

多环境是指在同一个集群中，同时部署多个版本的服务，并通过上下文系统，使得用户的请求可以被期望的版本处理的能力。

通常情况下，我们期望多版本同时运行，则需要在集群中为新的版本部署所有的应用服务，有了多环境支持，我们可以在集群中仅部署变更的服务即可，系统可以将请求路由到正确的服务中进行处理。

下图展示了多环境支持的架构图，不同的的线，代表着不同版本的请求的处理路径：

[![multienv](../img/multienv.png)](../img/multienv.png)

## 启用多环境支持

1. 添加依赖包

   ```
   dotnet add package NetCorePal.Extensions.MultiEnv
   dotnet add package NetCorePal.Extensions.MicrosoftServiceDiscovery  //如果使用Microsoft.Extensions.ServiceDiscovery作为服务发现
   ```

2. 添加环境上下文

    ```
    builder.Services.AddContext()  
        .AddEnvContext()    // 添加环境支持
        .AddTenantContext() 
        .AddCapContextProcessor(); 
    ```
   
3. 添加多环境支持

   可以使用`NetCorePalServiceDiscovery`或者`Microsoft.Extensions.ServiceDiscovery`作为多环境的服务发现支持
   ```csharp
   builder.Services.AddMultiEnv(options =>
     {
         options.ServiceName = "MyServiceName";
         options.ServiceEnv = "main";
     })
     .UseNetCorePalServiceDiscovery(); //使用NetCorePalServiceDiscovery作为多环境的服务发现支持
     //.UseMicrosoftServiceDiscovery();  //使用Microsoft.Extensions.ServiceDiscovery作为多环境的服务发现支持
   ```
   
4. 使用`Microsoft.Extensions.ServiceDiscovery`多环境支持
   
   如果你的服务发现使用的是`Microsoft.Extensions.ServiceDiscovery`，需要将`AddServiceDiscovery` 替换为 `AddMultiEnvMicrosoftServiceDiscovery`

    修改注册代码
    ```csharp
    // 注册Microsoft.Extensions.ServiceDiscovery
    builder.Services.AddMultiEnvMicrosoftServiceDiscovery()  // 将`AddServiceDiscovery` 替换为 `AddMultiEnvMicrosoftServiceDiscovery`
        .AddConfigurationServiceEndpointProvider();
    
    //注册HttpClient并配置服务发现
    builder.Services.AddHttpClient<CatalogClient>()
        .AddMultiEnvMicrosoftServiceDiscovery();   // 将`AddServiceDiscovery` 替换为 `AddMultiEnvMicrosoftServiceDiscovery`
    ```

   注意事项：目前基于`Microsoft.Extensions.ServiceDiscovery`的多环境的支持，依赖服务注册发现中可以明确获取到的服务列表，对于`PassThroughServiceEndpointProvider`的服务，不会被识别为有效的服务，因为无法确定该服务是否真的部署在集群中，这将会导致无法感知到灰度版本的存在，流量都会向默认版本发起。

5. 配置 `RabbitMQ` 多环境支持

   如果你在使用RabbitMQ，需要添加下面配置代码以支持多环境
   ```csharp
   builder.Services.AddEnvFixedConnectionChannelPool();
   ```
   
## 使用多环境

多环境系统主要通过环境上下文来传递请求的环境信息，因此可以在系统的入口处设置环境信息，例如在Yarp网关中，定义一个逻辑：

```csharp
 app.Use((context, next) =>
 {
     var contextAccessor = context.RequestServices.GetRequiredService<IContextAccessor>();
     contextAccessor.SetContext(new EnvContext("v2")); //可以根据当前登录用户决定环境版本
     return next();
 });
```

通过上面的设置，后续的所有请求，都会被路由到对应的`v2`版本的服务中进行处理，如果对应的服务不存在`v2`版本，则会路由到默认版本进行处理。





