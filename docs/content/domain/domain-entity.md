# 领域模型

## 介绍

领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。领域模型是DDD中的核心概念，它是对业务的抽象，是业务的核心。

## 定义领域模型

定义领域模型需要下列步骤：

1. 安装nuget包 `NetCorePal.Extensions.Domain`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain
    ```

2. 定义领域模型,需要：

    + 定义为模型定义强类型Id（可选），模型类型可根据需要自行定义；
    + 继承`NetCorePal.Extensions.Domain.Entity<T>`类，并指定模型Id类型；
    + 实现`IAggregateRoot`接口（可选），仅当模型被定义为聚合根时才需要标记该接口，从而使该模型支持作为仓储类型的范型参数；
    + 为模型定义一个空的受保护的构造函数，以支持EntityFrameworkCore框架在查询时构造领域模型实例；

    下面为一个示例：

    ```csharp
    // 定义领域模型
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;

    //为模型定义强类型ID
    public partial record UserId : IInt64StronglyTypedId;
    
    //领域模型
    public class User : Entity<UserId>, IAggregateRoot
    {
        protected User() { }
        public string Name { get; set; }
        public string Email { get; set; }
    }
