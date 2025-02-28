# 服务发现

当你需要发起服务间调用时，可以选择使用服务发现功能，框架默认提供了基于k8s的服务发现功能，同时也支持微软官方提供的`Microsoft.Extensions.ServiceDiscovery`组件。

## 使用 NetCorePalServiceDiscovery

1. 添加依赖包

    ```
    dotnet add package NetCorePal.Extensions.ServiceDiscovery.Abstractions
    dotnet add package NetCorePal.Extensions.ServiceDiscovery.K8s   //如果需要，可以添加该包以支持k8s环境下的服务发现
    ```

2. 在 `Program.cs` 中配置服务发现

    ```csharp
    // 注册NetCorePalServiceDiscovery
    builder.Services.AddNetCorePalServiceDiscoveryClient()
        .AddK8sServiceDiscovery(); //如果需要，可以添加添加k8s服务发现支持
    ```

3. 注册`HttpClient`

   ```csharp
    //注册HttpClient并配置服务发现
    builder.Services.AddHttpClient<CatalogClient>()
        .AddNetCorePalServiceDiscovery("catalog");
    ```


## 使用 Microsoft.Extensions.ServiceDiscovery

1. 添加依赖包

    ```
    dotnet add package Microsoft.Extensions.ServiceDiscovery
    ```
   
2. 在 `Program.cs` 中配置服务发现

    ```csharp
    // 注册Microsoft.Extensions.ServiceDiscovery
    builder.Services.AddServiceDiscovery()
        .AddConfigurationServiceEndpointProvider();
    ```

3. 注册`HttpClient`

   ```csharp
    //注册HttpClient并配置服务发现
    builder.Services.AddHttpClient<CatalogClient>()
        .AddServiceDiscovery();
    ```
   
更多关于`Microsoft.Extensions.ServiceDiscovery`详见官方文档：[Microsoft.Extensions.ServiceDiscovery](https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery)

## 推荐用法

推荐使用`Refit`组件来简化`HttpClient`的使用，同时可以结合`Polly`组件实现重试、熔断等功能。

1. 添加依赖包

    ```
    dotnet add package Refit.HttpClientFactory
    dotnet add package Microsoft.Extensions.Http.Resilience
    ```
   
2. 定义远程服务接口

   ```csharp
   public interface ICatalogApi
   {
      [Get("/api/v1/catalog")]
      Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync();
   }
   ```
   
3. 注册`HttpClient`

    ```csharp
    
    RefitSettings refitSettings = new RefitSettings();
    var jsonOptions = new JsonSerializerOptions();
    jsonOptions.AddNetCorePalJsonConverters();   //配置NetCorePalJsonConverters
    var serializer = new SystemTextJsonContentSerializer(jsonOptions); 
    
    builder.Services.AddRefitClient<ICatalogApi>(_ => refitSettings)  //添加RefitClient
        .ConfigureHttpClient(p => p.BaseAddress = new Uri("https://catalog:5000"))
        .AddPalServiceDiscovery  或者使用Microsoft.Extensions.ServiceDiscovery
        //.AddNetCorePalServiceDiscovery("catalog")   //使用NetCorePalServiceDiscovery 
        .AddStandardResilienceHandler(); //添加标准弹性策略
    ```