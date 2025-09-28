# Context Transmission System

## What is the Context Transmission System

The context transmission system is a system used to transmit context, allowing the context to be transmitted throughout the lifecycle of a request, so that we can access the context anywhere within the lifecycle of a request.

## How to Inject Context

In `Program.cs`, we can inject the context with the following code:

1. Inject context

    ```csharp
    builder.Services.AddContext()  // Add context
        .AddEnvContext()    // Add environment support
        .AddTenantContext() // Add tenant support
        .AddCapContextProcessor(); // Add support for context transmission in CAP to ensure that the integrated event processor can correctly identify the context
    ```

2. Add context middleware

    ```csharp
    var app = builder.Build();
    
    app.UseContext();
    ```

## How to Use Context

We can use the context anywhere, such as in an Endpoint:

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
        // Get context
        var contextAccessor = Resolve<IContextAccessor>();
        var tenantContext = contextAccessor.GetContext<TenantContext>();

        // Set context
        var tenantContext2 = new TenantContext("112233");
        contextAccessor.SetContext(tenantContext2);

        await SendOkAsync(ct);
    }
}
```

Note: You can set the context in one service and then get the context in another service. The context is transmitted within the lifecycle of a request and can be transmitted across services.

## Default Provided Context Types

The package `NetCorePal.Context.Shared` provides some default context types, such as:

1. Tenant Context

    Type: `NetCorePal.Context.TenantContext`, used to transmit the tenant information of the current request to support the implementation of a multi-tenant system.

2. Environment Context

    Type: `NetCorePal.Context.EnvContext`, used to transmit the grayscale environment information of the current request to support the implementation of grayscale release.

## Supported Transmission Scenarios

By default, the context transmission system supports the following scenarios:

1. Support for transmitting context in HTTP requests

    Supports automatically transmitting context information when the HttpClient object initiates a request, and automatically parsing context information when aspnetcore receives a request.

2. Support for transmitting context in CAP

    Supports automatically carrying context information when sending events in CAP, and automatically parsing context information when receiving events in CAP.