# RowVersion

## What is RowVersion?

`RowVersion` is a mechanism used to solve concurrency issues, an implementation of optimistic locking based on relational database features. In a concurrent environment, multiple users may change the same row of data simultaneously. If not restricted, operations between different users may overwrite each other, leading to data inconsistency. `RowVersion` is designed to solve this problem.

## Implementation Principle of RowVersion

The implementation principle of `RowVersion` is to add a version number field to each row of data. Each time an update operation is performed on this row of data, the version number is incremented by 1. At the beginning of the transaction, the transaction reads the current version number of the row. When executing `update`, it updates the `RowVersion` and adds a condition like `row_version=@oldversion` in the `where` clause. Before committing the transaction, it checks whether the number of affected rows is as expected. If it is, the transaction is committed; otherwise, the transaction is rolled back.

## Usage Scenarios of RowVersion

`RowVersion` is mainly used to solve concurrency issues. For example, in an order system, when multiple users operate on the same order simultaneously, it may lead to inconsistent order status. By using row version numbers, such problems can be avoided.

## Define Row Version Number

In the domain model, define a `public` readable property of type `NetCorePal.Extensions.Domain.RowVersion` to implement the row version number function. The framework will automatically handle the logic of updating the row version number and concurrency checks.

Here is an example:

```csharp
// Define row version number
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

// Define strongly typed ID for the model
public partial record OrderId : IInt64StronglyTypedId;

// Domain model
public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }
    public string OrderNo { get; private set; } = string.Empty;
    public bool Paid { get; private set; }
    // Define row version number
    public RowVersion Version { get; private set; } = new RowVersion();

    public void SetPaid()
    {
        Paid = true;
    }
}
```

## Scenarios Where RowVersion is Not Suitable

`RowVersion` is suitable for scenarios with low concurrency. If the concurrency is high, it may lead to a large number of row version number conflicts, affecting user experience. In such cases, consider using `pessimistic locking` to solve concurrency issues.