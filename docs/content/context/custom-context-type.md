# 自定义上下文类型

当内置的上下文类型不满足需求时，可以参照EnvContext的实现，实现自己的上下文类型。

## 如何实现自定义上下文类型


1. 定义上下文类型

    ```csharp
    
    public class CustomContext
    {
        //上下文会被存储为一个key-value的形式，这个key就是ContextKey
        public static string ContextKey { get; set; } = "x-custom-context"; 
    
        public CustomContext(string data)
        {
            Data = data;
        }
    
        public string Data { get; private set; }
    }
    
    ``` 

2. 实现`IContextCarrierHandler`接口

    ```csharp
    
    public class CustomContextCarrierHandler : IContextCarrierHandler
    {
        public Type ContextType => typeof(CustomContext);
    
        public void Inject(IContextCarrier carrier, object? context)
        {
            if (context != null)
            {
                carrier.Set(CustomContext.ContextKey, ((CustomContext)context).Data);
            }
        }
    
        public object? Initial()
        {
            return null;
        }
    }
    ```

3. 实现`IContextSourceHandler`接口

    ```csharp
    public class CustomCContextSourceHandler : IContextSourceHandler
    {
        public Type ContextType => typeof(CustomContext);
    
        public object? Extract(IContextSource source)
        {
            var data = source.Get(CustomContext.ContextKey);
            return string.IsNullOrEmpty(data) ? null : new CustomContext(data);
        }
    }
    ```

4. 添加注册上下文类型

    ```csharp
    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomContext(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddSingleton<IContextCarrierHandler, CustomContextCarrierHandler>();
            services.TryAddSingleton<IContextSourceHandler, CustomContextSourceHandler>();
            return services;
        }
    }
    ```
   
5. 在`Program.cs`注册上下文类型

    ```csharp
    builder.Services.AddContext()  
        .AddEnvContext()    
        .AddTenantContext() 
        .AddCustomContext()  // 添加自定义上下文
        .AddCapContextProcessor(); 
    
    ```
