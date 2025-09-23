# Migration Guide: Existing Resources to Framework Localization

This document shows how to migrate existing project-specific resource files to use the centralized NetCorePal.Extensions.Localization framework.

## Current State

Currently, various projects have their own R.resx files:
- `src/DistributedTransactions.CAP/R.resx` 
- `src/DistributedTransactions.CAP.SqlServer/R.resx`
- `src/DistributedTransactions.CAP.MySql/R.resx`
- etc.

## Migration Strategy

### Option 1: Centralize Common Messages

Move commonly used messages to the framework localization:

**Before (in project-specific R.resx):**
```xml
<data name="InvalidTableName" xml:space="preserve">
    <value>Invalid Table Name: {0}</value>
</data>
```

**After (in framework SharedResource.resx):**
```xml
<data name="InvalidTableName" xml:space="preserve">
    <value>Invalid table name: {0}</value>
</data>
```

**Code update:**
```csharp
// Before
throw new ArgumentException(string.Format(R.InvalidTableName, table), nameof(table));

// After (once project references NetCorePal.Extensions.Localization)
throw new ArgumentException(FrameworkLocalizer.GetString("InvalidTableName", table), nameof(table));
```

### Option 2: Keep Project-Specific Resources

For truly project-specific messages, keep the existing R.resx files but ensure they follow the same pattern.

## Implementation Steps

### 1. Add Framework Localization Reference

Add to each project that needs localization:

```xml
<ProjectReference Include="..\Localization\NetCorePal.Extensions.Localization.csproj" />
```

### 2. Decide on Message Categorization

**Framework Messages (move to SharedResource):**
- ValidationRequired, ValidationInvalidFormat, etc.
- OperationSuccessful, OperationFailed
- NotFound, Unauthorized, Forbidden
- Common domain messages

**Project-Specific Messages (keep in local R.resx):**
- TransactionNotSupport (CAP-specific)
- RepeatAddition (CAP-specific)
- Database/storage specific errors

### 3. Update Resource Usage

For framework messages:
```csharp
// Instead of local R.TransactionNotSupport
throw new NotImplementedException(FrameworkLocalizer.GetString("OperationNotSupported"));
```

For project-specific messages:
```csharp
// Keep using local R for truly project-specific messages
throw new InvalidOperationException(R.RepeatAddition);
```

### 4. Add Localization Initialization

In projects that use FrameworkLocalizer directly (not through ASP.NET Core), initialize it:

```csharp
public static class ProjectInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        FrameworkLocalizer.Initialize(serviceProvider);
    }
}
```

## Benefits of Migration

1. **Consistency**: All framework components use the same error messages
2. **Maintainability**: Single source of truth for common messages
3. **Localization**: Automatic support for multiple languages
4. **Reduced Duplication**: No need to duplicate common messages across projects

## Backward Compatibility

The migration can be done gradually:
- Existing R.resx files continue to work
- New code can use FrameworkLocalizer
- Projects can migrate message by message

## Example: CAP Project Migration

### Current CAP Messages

```csharp
// DistributedTransactions.CAP/R.resx
InvalidTableName: "Invalid Table Name: {0}"
TransactionNotSupport: "Transaction is not supported in this method."
RepeatAddition: "When using UseNetCorePalStorage, there is no need to additionally register UseMySql, UsePostgreSql, UseSqlServer, etc."
```

### Recommended Migration

**Move to Framework:**
- InvalidTableName → Use framework "InvalidEntityId" or create new "InvalidTableName"

**Keep Project-Specific:**
- TransactionNotSupport (CAP-specific behavior)
- RepeatAddition (CAP-specific configuration error)

### Updated CAP Code

```csharp
// For common validation errors - use framework
throw new ArgumentException(FrameworkLocalizer.GetString("InvalidTableName", table), nameof(table));

// For CAP-specific errors - keep local
throw new NotImplementedException(R.TransactionNotSupport);
throw new InvalidOperationException(R.RepeatAddition);
```

This approach balances centralization of common messages with keeping truly project-specific messages local.