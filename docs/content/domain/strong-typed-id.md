# 强类型实体ID

## 介绍

强类型实体ID是为了解决实体ID类型不一致的问题，比如有的实体ID是Int64类型，有的实体ID是Guid类型，有的实体ID是Int32类型，有的实体ID是String类型，这样会导致实体ID的类型不一致，不方便使用，强类型实体ID就是为了解决这个问题。

目前支持的强类型实体ID类型有：

- Int32
- Int64
- Guid
- String

## 使用

强类型id需要关键字 `public`、`partial` 和`record` 来修饰，同时需要实现 `IInt32StronglyTypedId`、`IInt64StronglyTypedId`、`IGuidStronglyTypedId`、`IStringStronglyTypedId` 接口之一。

1. 项目添加引用 `NetCorePal.Extensions.Domain.Abstractions` 包。

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```

2. 您可以编写类似下列代码来实现强类型实体ID：

    ```csharp
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;

    public partial record Int64OrderId : IInt64StronglyTypedId;
    ```

    下面代码则由`SourceGenerator`自动生成：

    ```csharp
    using NetCorePal.Extensions.Domain;
    using System;
    using System.ComponentModel;
    namespace YourNamespace;

    [TypeConverter(typeof(EntityIdTypeConverter<Int64OrderId, Int64>))]
    public partial record Int64OrderId(Int64 Id) : IInt64StronglyTypedId
    {
        public static implicit operator Int64(Int64OrderId id) => id.Id;
        public static implicit operator Int64OrderId(Int64 id) => new Int64OrderId(id);
        public override string ToString()
        {
            return Id.ToString();
        }
    }
    ```

    更多示例：

    ```csharp
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;


    // Int32 强类型实体ID
    public partial record Int32OrderId : IInt32StronglyTypedId;

    // Guid 强类型实体ID
    public partial record GuidOrderId : IGuidStronglyTypedId;

    // String 强类型实体ID
    public partial record StringOrderId : IStringStronglyTypedId;
    ```

## Json序列化支持

提供了强类型实体ID与字符串之间的序列化和反序列化支持，您可以使用 `JsonStronglyTypedIdConverter` 来实现。

1. 对于`System.Text.Json`，您可以使用下面代码来实现：

    ```csharp
    using NetCorePal.Extensions.Domain.Json;
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddMvc().AddControllersAsServices().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverterFactory());
    });
    ```

2. 对于`Newtonsoft.Json`，您可以使用下面代码来实现：

    添加`NetCorePal.Extensions.AspNetCore`包

    ```bash
    dotnet add package NetCorePal.Extensions.AspNetCore
    ```

    添加下面代码到`Startup.cs`文件中：

    ```csharp
    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
    });
    ```

    下面示例表示序列化与反序列化的效果：

    ```csharp
    JsonSerializerOptions options = new();
    options.Converters.Add(new EntityIdJsonConverterFactory());

    var id = JsonSerializer.Deserialize<OrderId1>("\"12\"", options);
    Assert.NotNull(id);
    Assert.True(id.Id == 12);
    var id2 = new OrderId2(2);
    var json = JsonSerializer.Serialize(id2, options);
    Assert.Equal("\"2\"", json);
    ```
