# Data Sharding Solution

To enable the system to scale to larger data capacities, we have integrated [sharding-core](https://github.com/dotnetcore/sharding-core) as the data sharding solution.

`sharding-core` is an open-source sharding framework based on `EntityFrameworkCore`. It supports multiple database types and sharding strategies. As a client-side solution, it has low requirements for database infrastructure, making it suitable for most scenarios. It supports various sharding strategies, including range sharding, hash sharding, and composite sharding. It also supports dynamic sharding and dynamic routing, allowing flexible adjustments based on business needs.

The workflow of `sharding-core` is shown in the following diagram:

![sharding-core workflow](../img/sharding-core.jpeg)

## Supported Scenarios

+ [Read-Write Separation](read-write-separation.md)
+ [Table Sharding](sharding-table.md)
+ [Database Sharding](sharding-database.md)
+ [Tenant Mode](sharding-tenant.md)

`Read-Write Separation`, `Table Sharding`, and `Database Sharding` can be combined as needed.

## Official Documentation

For more information about `sharding-core`, refer to the official documentation: https://xuejmnet.github.io/sharding-core-doc/

## Notes

### Lazy Loading Compatibility

You need to add `o.UseEntityFrameworkCoreProxies = true;`.

See: https://github.com/dotnetcore/sharding-core/issues/259