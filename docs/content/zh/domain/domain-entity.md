# 领域模型

## 介绍

领域模型是指在软件开发中，对特定领域或业务进行抽象和建模的表示方式。它描述了领域内的实体、概念、关系和行为，并提供了一种可交互和可执行的表示。领域模型通常是通过使用面向对象编程的概念来实现的，例如类、对象、属性和方法等。

在领域驱动设计（Domain-Driven Design，简称DDD）中，强调将领域模型作为核心组件来指导系统的设计与实现。DDD鼓励开发团队与领域专家密切合作，共同探索和理解领域的复杂性，并将这些理解转化为可维护和可扩展的软件系统。

领域驱动设计中的领域模型是通过对领域专家的知识进行建模而产生的。它不仅用于解决问题域的分析和设计，还可以在软件系统中实现业务逻辑。领域模型与领域驱动设计密切相关，因为领域模型是领域驱动设计的核心要素之一，帮助开发人员理解和应对复杂的业务场景，从而更好地构建高质量的软件系统。

## 定义领域模型

定义领域模型需要下列步骤：

1. 安装nuget包 `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
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
        public string Name { get; private set; }
        public string Email { get; set; }
    }
    ```
   
## 领域模型必须

- 领域模型的属性必须对外只读，内部可写（private set）；
- 领域模型的属性变更必须通过模型实例方法；
- 领域模型必须有一个 `protected` 无参构造函数，用于支持EntityFrameworkCore框架在查询时构造领域模型实例；
- 领域模型必须继承 `IEntity` 或 `Entity<TKey>` 类；
- 领域模型的方法必须由`CommandHandler`调用；

## 领域模型可以

- 领域模型可以实现 `IAggregateRoot` 接口，以表示该模型是一个聚合根；
- 领域模型的属性和方法，用于描述领域内的实体、概念、关系和行为；
- 领域模型的构造函数，用于初始化领域模型的属性；
- 领域模型的事件，用于描述领域模型的状态变化；
- 领域模型的规则，用于验证领域模型的合法性；

## 领域模型不要

- 不要在领域模型中引用外部资源，如数据库连接、文件系统等；
- 不要在领域模型中处理与业务无关的逻辑，如日志记录、异常处理等；
- 不要在领域模型中直接调用外部服务，如Web API、消息队列等；
- 不要在领域模型中处理与业务无关的数据，如配置信息、环境变量等；
- 不要在领域模型中处理与业务无关的状态，如会话信息、用户信息等；
- 不要在领域模型中处理与业务无关的行为，如跟踪、监控等；
- 不要在领域模型中处理与业务无关的异常，如网络异常、数据库异常等；
- 不要在领域模型中处理与业务无关的事件，如定时任务、消息通知等；
