# 集成事件转换器

集成事件转换器将领域事件发送成集成事件的转换工具。

通过集成事件转换器可以将要发送集成事件与其他领域事件处理器中业务逻辑解耦。

## 定义集成事件转换器

1. 安装nuget包 `NetCorePal.Extensions.DistributedTransactions.Abstractions`

   ```bash
   dotnet add package NetCorePal.Extensions.DistributedTransactions.Abstractions
   ```

2. 定义集成事件转换器，需要：

   + 继承`NetCorePal.Extensions.DistributedTransactions.IIntegrationEventConverter`接口；
   + 实现IIntegrationEventConverter接口的Convert方法，该方法入参为领域事件IDomainEvent，返参为集成事件IntegrationEvent

​		下面为一个示例：

```c#
// 定义领域事件
using NetCorePal.Web.Application.IntegrationConvert;
namespace YourNamespace;

public class OrderCreatedIntegrationEventConverter : IIntegrationEventConverter<OrderCreatedDomainEvent,OrderPaidIntegrationEvent>{
    public OrderPaidIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderPaidIntegrationEvent(domainEvent.Order.Id);
    }
}
```