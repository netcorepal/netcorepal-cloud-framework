# Service Discovery

When you need to initiate inter-service calls, you can choose to use the service discovery feature. The framework provides service discovery based on k8s by default and also supports the `Microsoft.Extensions.ServiceDiscovery` component provided by Microsoft.

## Using NetCorePalServiceDiscovery

1. Add dependency packages

    ```
    dotnet add package NetCorePal.Extensions.ServiceDiscovery.Abstractions
    dotnet add package NetCorePal.Extensions.ServiceDiscovery.K8s   // If needed, add this package to support service discovery in k8s environments
    ```

2. Configure service discovery in `Program.cs`

    ```csharp
    // Register NetCorePalServiceDiscovery
    builder.Services.AddNetCorePalServiceDiscoveryClient()
        .AddK8sServiceDiscovery(); // If needed, add k8s service discovery support
    ```

3. Register `HttpClient`

   ```csharp
    // Register HttpClient and configure service discovery
    builder.Services.AddHttpClient<CatalogClient>()
        .AddNetCorePalServiceDiscovery("catalog");
    ```


## Using Microsoft.Extensions.ServiceDiscovery

1. Add dependency packages

    ```
    dotnet add package Microsoft.Extensions.ServiceDiscovery
    ```
   
2. Configure service discovery in `Program.cs`

    ```csharp
    // Register Microsoft.Extensions.ServiceDiscovery
    builder.Services.AddServiceDiscovery()
        .AddConfigurationServiceEndpointProvider();
    ```

3. Register `HttpClient`

   ```csharp
    // Register HttpClient and configure service discovery
    builder.Services.AddHttpClient<CatalogClient>()
        .AddServiceDiscovery();
    ```
   
For more information about `Microsoft.Extensions.ServiceDiscovery`, see the official documentation: [Microsoft.Extensions.ServiceDiscovery](https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery)

## Recommended Usage

It is recommended to use the `Refit` component to simplify the use of `HttpClient`, and combine it with the `Polly` component to achieve retry, circuit breaker, and other functionalities.

1. Add dependency packages

    ```
    dotnet add package Refit.HttpClientFactory
    dotnet add package Microsoft.Extensions.Http.Resilience
    ```
   
2. Define the remote service interface

   ```csharp
   public interface ICatalogApi
   {
      [Get("/api/v1/catalog")]
      Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync();
   }
   ```
   
3. Register `HttpClient`

    ```csharp
    
    RefitSettings refitSettings = new RefitSettings();
    var jsonOptions = new JsonSerializerOptions();
    jsonOptions.AddNetCorePalJsonConverters();   // Configure NetCorePalJsonConverters
    var serializer = new SystemTextJsonContentSerializer(jsonOptions); 
    
    builder.Services.AddRefitClient<ICatalogApi>(_ => refitSettings)  // Add RefitClient
        .ConfigureHttpClient(p => p.BaseAddress = new Uri("https://catalog:5000"))
        .AddPalServiceDiscovery  or use Microsoft.Extensions.ServiceDiscovery
        //.AddNetCorePalServiceDiscovery("catalog")   // Use NetCorePalServiceDiscovery 
        .AddStandardResilienceHandler(); // Add standard resilience policy
    ```