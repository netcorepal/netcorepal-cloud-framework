# JSON Serialization

## Introduction

The framework has built-in strongly typed IDs such as `StronglyTypedId`, `RowVersion`, `UpdateTime`, etc., to assist modeling. To better support the serialization of these types in JSON, the framework provides `JsonConverter` to support the serialization of these types.

It supports both `Newtonsoft.Json` and `System.Text.Json` JSON serialization libraries.

## How to Use

### System.Text.Json

Add the following code in `Program.cs`:

```csharp
builder.Services.AddMvc()
  .AddNetCorePalSystemTextJson();
```

### Newtonsoft.Json

Add the following code in `Program.cs`:

```csharp
builder.Services.AddMvc()
  .AddNetCorePalNewtonsoftJson();
```

Note: Support for `Newtonsoft.Json` requires referencing the `NetCorePal.Extensions.NewtonsoftJson` package.

## Other Scenarios

If you need to use it in other scenarios, you can directly use the `AddNetCorePalJsonConverters` extension method to add `EntityIdJsonConverterFactory`, `RowVersionJsonConverter`, `UpdateTimeJsonConverter`, and other converters.

### System.Text.Json
```csharp
using System.Text.Json;

var options = new JsonSerializerOptions();
options.AddNetCorePalJsonConverters();
```

### Newtonsoft.Json
```csharp
using Newtonsoft.Json;

var settings = new JsonSerializerSettings();
settings.AddNetCorePalJsonConverters();
```
