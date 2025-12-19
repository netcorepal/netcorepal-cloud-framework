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

// Configure CAP with MongoDB-specific storage
services.AddCap(x =>
{
    x.UseMongoDBNetCorePalStorage<YourDbContext>();  // Use MongoDB-specific implementation
    // ... other CAP configurations
});
```

## Implementation Details

### MongoDB EF Core Provider Workaround

The MongoDB EF Core provider (version 8.2.0/9.0.0) does not support:
- `ExecuteUpdate()` / `ExecuteUpdateAsync()` - Bulk update operations
- `ExecuteDelete()` / `ExecuteDeleteAsync()` - Bulk delete operations

This package provides a **MongoDB-specific storage implementation** (`MongoDBNetCorePalDataStorage`) that works around these limitations by using an optimized `Attach`/`Update`/`Delete` approach:

1. **State Changes**: Constructs entities in memory with the ID and updated properties, attaches them to the context, marks specific properties as modified, and calls `SaveChangesAsync()`
2. **Deletions**: Constructs entities in memory with only the ID, attaches them, and uses `Remove()` or `RemoveRange()` before saving

This approach **avoids unnecessary database queries** by not loading the full entity before updating/deleting.

### Performance Considerations

The optimized workaround approach:
- ✅ **Fully functional** - All CAP operations work correctly
- ✅ **Optimized** - No extra database queries to load entities before updates/deletes
- ✅ **Efficient** - Only fetches IDs when deleting batches, constructs entities in memory
- ✅ **Compatible** - Works with current MongoDB.EntityFrameworkCore provider

For extremely high-throughput scenarios, PostgreSQL, MySQL, or SQL Server may still offer better performance with native bulk operations.

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
