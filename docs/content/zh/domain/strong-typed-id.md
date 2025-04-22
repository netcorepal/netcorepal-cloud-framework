# 强类型实体ID

## 介绍

在领域驱动设计建模的过程中，实体ID是非常重要的，它是实体的唯一标识，是实体的主键，是实体的重要属性，通常情况下我们会使用`int`、`long`、`Guid`、`string`等类型来定义实体ID，但在系统中使用这些类型时，会出现下列问题：

1. 当定义一个ID值时，无法从类型上判断它代表实体ID还是其它类型，因此代码的可读性会变差
2. 在为一个实体上引用的ID字段赋值时，容易出现将非预期的其它类型值赋值给ID字段，从而导致错误

为了解决上述问题，我们推荐使用`强类型实体ID`，强类型实体ID是基于基础类型的封装，目前支持的基础类型有：

- Int32
- Int64
- Guid
- String

## 如何使用

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

在服务间调用、WebAPI等场景中，通常会涉及到实体类型与Json字符串之间的序列化和反序列化，为了使得强类型实体ID能够在这些场景中正常工作，我们提供了基于`System.Text.Json`和`Newtonsoft.Json`的序列化支持。

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

注意： 强类型实体ID都会当作字符串来序列化。

## ID 生成

关于 ID 生成，见 [ID 生成](../data/id-generator.md) 文档。