# Json序列化

## 介绍

框架内置了强类型ID`StronglyTypedId`、`RowVersion`、`UpdateTime`等类型来辅助建模，为了使这些类型在Json序列化更好地支持，框架提供了`JsonConverter`来支持这些类型的序列化。

同时支持了`Newtonsoft.Json`和`System.Text.Json`两种Json序列化库。

## 如何使用

### System.Text.Json

在`Program.cs`中添加如下代码：

```csharp

builder.Services.AddMvc()
  .AddNetCorePalSystemTextJson();

```

### Newtonsoft.Json




在`Program.cs`中添加如下代码：

```csharp

builder.Services.AddMvc()
  .AddNetCorePalNewtonsoftJson();

```

备注： `Newtonsoft.Json`的支持需要引用`NetCorePal.Extensions.NewtonsoftJson`包。


## 其它场景

如果需要在其它场景中使用，可以直接使用`AddNetCorePalJsonConverters`扩展方法添加`EntityIdJsonConverterFactory`、`RowVersionJsonConverter`、`UpdateTimeJsonConverter`等Converter。

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
