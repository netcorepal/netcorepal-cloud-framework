# 工作单元模式（UnitOfWork）

工作单元模式是一种设计模式，它将多个操作组合成一个单元，以便在一个事务中执行。这种模式有助于管理事务，确保所有操作都成功或失败。

可以参考这篇文章来了解更多关于工作单元模式的知识：[工作单元模式](https://arifoglutarik.medium.com/unit-of-work-and-repository-patterns-with-dependency-injection-in-c-060184ccc21e)

## 使用工作单元模式

默认情况下，框架自动管理了数据库事务的生命周期，具体可以参考 [事务处理](../transactions/transactions.md) 章节。

如果你需要自行管理事务，框架提供了接口 `IUnitOfWork` 和 `ITransactionUnitOfWork` 的定义和实现。

1. 获取工作单元实例

    ```csharp
    //通过构造函数注入
    public class MyService
    {
         private readonly IUnitOfWork _unitOfWork;
    
         public MyService(IUnitOfWork unitOfWork)
         {
              _unitOfWork = unitOfWork;
         }
    }
       
    //通过容器获取
    using var scope = _serviceProvider.CreateScope();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var transactionUnitOfWork = scope.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
    
    
    //通过仓储实例获取 IUnitOfWork
    public class MyService
    {
         private readonly IUnitOfWork _unitOfWork;
         private readonly IOrderRepository _orderRepository;
    
         public MyService(IOrderRepository orderRepository)
         {
              _orderRepository = orderRepository;
              _unitOfWork = orderRepository.UnitOfWork;
         }
    }
    ```
   注意： 接口 `IUnitOfWork` 和 `ITransactionUnitOfWork` 生命周期都是`Scoped`，需要在`Scoped`作用域中使用。

2. 使用工作单元管理事务

    如果需要自行管理事务，可以通过 `ITransactionUnitOfWork` 接口来实现。
    ```csharp
    using var transaction = transactionUnitOfWork.BeginTransactionAsync();
    try
    {
        //执行数据库操作
        await _unitOfWork.SaveEntitiesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
    }
    ```

    
