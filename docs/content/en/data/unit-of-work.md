# Unit of Work Pattern

The Unit of Work pattern is a design pattern that combines multiple operations into a single unit to be executed within a transaction. This pattern helps manage transactions, ensuring that all operations either succeed or fail.

You can refer to this article to learn more about the Unit of Work pattern: [Unit of Work Pattern](https://arifoglutarik.medium.com/unit-of-work-and-repository-patterns-with-dependency-injection-in-c-060184ccc21e)

## Using the Unit of Work Pattern

By default, the framework automatically manages the lifecycle of database transactions. For more details, refer to the [Transactions](../transactions/transactions.md) section.

If you need to manage transactions manually, the framework provides the `IUnitOfWork` and `ITransactionUnitOfWork` interfaces and their implementations.

1. Obtain an instance of the Unit of Work

    ```csharp
    // Through constructor injection
    public class MyService
    {
         private readonly IUnitOfWork _unitOfWork;
    
         public MyService(IUnitOfWork unitOfWork)
         {
              _unitOfWork = unitOfWork;
         }
    }
       
    // Through the service provider
    using var scope = _serviceProvider.CreateScope();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var transactionUnitOfWork = scope.ServiceProvider.GetRequiredService<ITransactionUnitOfWork>();
    
    
    // Obtain IUnitOfWork through a repository instance
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
   Note: The `IUnitOfWork` and `ITransactionUnitOfWork` interfaces have a `Scoped` lifetime and should be used within a `Scoped` scope.

2. Use the Unit of Work to manage transactions

    If you need to manage transactions manually, you can use the `ITransactionUnitOfWork` interface.
    ```csharp
    using var transaction = transactionUnitOfWork.BeginTransactionAsync();
    try
    {
        // Perform database operations
        await _unitOfWork.SaveEntitiesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
    }
    ```


