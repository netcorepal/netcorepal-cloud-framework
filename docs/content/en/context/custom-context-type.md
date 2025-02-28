# Custom Context Type

When the built-in context types do not meet the requirements, you can refer to the implementation of EnvContext to implement your own context type.

## How to Implement a Custom Context Type

1. Define the context type

    ```csharp
    public class CustomContext
    {
        // The context will be stored as a key-value pair, and this key is the ContextKey
        public static string ContextKey { get; set; } = "x-custom-context"; 
    
        public CustomContext(string data)
        {
            Data = data;
        }
    
        public string Data { get; private set; }
    }
    ``` 

2. Implement the `IContextCarrierHandler` interface

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

3. Implement the `IContextSourceHandler` interface

    ```csharp
    public class CustomContextSourceHandler : IContextSourceHandler
    {
        public Type ContextType => typeof(CustomContext);
    
        public object? Extract(IContextSource source)
        {
            var data = source.Get(CustomContext.ContextKey);
            return string.IsNullOrEmpty(data) ? null : new CustomContext(data);
        }
    }
    ```

4. Add context type registration

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
   
5. Register the context type in `Program.cs`

    ```csharp
    builder.Services.AddContext()  
        .AddEnvContext()    
        .AddTenantContext() 
        .AddCustomContext()  // Add custom context
        .AddCapContextProcessor(); 
    ```
