# NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB

MongoDB Entity Framework Core provider for NetCorePal CAP distributed transactions.

## Overview

This package provides MongoDB database support for the NetCorePal CAP distributed transactions framework using MongoDB.EntityFrameworkCore.

## Usage

```csharp
// Configure DbContext
services.AddDbContext<YourDbContext>(options =>
{
    options.UseMongoDB(connectionString, databaseName);
});

// Configure CAP with MongoDB storage
services.AddCap(x =>
{
    x.UseNetCorePalStorage<YourDbContext>();
    // ... other CAP configurations
});
```

## Known Limitations

⚠️ **Important**: As of MongoDB.EntityFrameworkCore version 8.2.0/9.0.0, the provider has the following limitations that affect this package:

### Unsupported Operations

The MongoDB EF Core provider does not currently support:
- `ExecuteUpdate()` / `ExecuteUpdateAsync()` - Bulk update operations
- `ExecuteDelete()` / `ExecuteDeleteAsync()` - Bulk delete operations

These operations are extensively used by the NetCorePal CAP storage implementation for performance optimization. As a result, the following operations will fail:
- Changing publish message state
- Bulk status updates
- Message cleanup operations

### Workaround

Until MongoDB.EntityFrameworkCore adds support for these operations, consider:
1. Using a different database provider (PostgreSQL, MySQL, SQL Server, SQLite) for CAP storage
2. Waiting for MongoDB EF Core provider updates
3. Implementing a custom MongoDB storage provider that doesn't rely on EF Core

### Tracking

This limitation is being tracked in the MongoDB.EntityFrameworkCore repository. The provider is still in active development and these features may be added in future releases.

## Implementation Structure

This implementation follows the same structure as other database providers in the framework:
- `IMongoDBCapDataStorage` - Interface for MongoDB CAP data storage
- Entity Type Configurations for MongoDB collections:
  - `PublishedMessageConfiguration`
  - `ReceivedMessageConfiguration`
  - `CapLockConfiguration`

## Requirements

- .NET 8.0, 9.0, or 10.0
- MongoDB.EntityFrameworkCore 8.2.0+ / 9.0.0+
- MongoDB Server 4.0+
