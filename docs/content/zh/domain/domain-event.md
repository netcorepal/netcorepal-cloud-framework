# 领域事件

领域事件是领域模型中的一个重要概念，它是领域模型中的一种通信机制，用于在领域模型之间传递消息。

领域事件仅包含描述事件发生时领域模型中的数据，不包含任何业务逻辑，业务规则和业务流程。

## 定义领域事件

1. 安装nuget包 `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```
   
2. 定义领域事件，需要：

    + 继承`NetCorePal.Extensions.Domain.IDomainEvent`接口；
    + 为领域事件定义一个空的构造函数，以支持序列化和反序列化；
    + 为领域事件定义一个公共的构造函数，用于初始化领域事件的属性；
    + 为领域事件定义一个公共的属性，用于描述事件发生时领域模型中的数据；
    
    下面为一个示例：

    ```csharp
    // 定义领域事件
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;
   
    public record UserCreatedDomainEvent(User user) : IDomainEvent;
    ```

## 领域事件必须

- 领域事件必须由领域模型发出；
- 领域事件必须是不可变的；

## 领域事件可以

- 使用`record`关键字定义领域事件；

## 领域事件不要

- 不要包含业务逻辑
- 不要包含业务规则
- 不要包含业务流程