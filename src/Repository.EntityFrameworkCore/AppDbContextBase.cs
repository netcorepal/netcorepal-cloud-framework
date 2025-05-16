using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives.Diagnostics;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public abstract class AppDbContextBase : DbContext, ITransactionUnitOfWork
{
    private static readonly DiagnosticListener _diagnosticListener =
        new(NetCorePalDiagnosticListenerNames.DiagnosticListenerName);

    private readonly IMediator _mediator;
    
    public IMediator Mediator => _mediator;

    protected AppDbContextBase(DbContextOptions options, IMediator mediator) :
        base(options)
    {
        _mediator = mediator;
    }


    protected virtual void ConfigureStronglyTypedIdValueConverter(ModelConfigurationBuilder configurationBuilder)
    {
    }

    protected virtual void ConfigureNetCorePalTypes(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        foreach (var clrType in modelBuilder.Model.GetEntityTypes().Select(p => p.ClrType))
        {
            var properties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(RowVersion))
                {
                    modelBuilder.Entity(clrType)
                        .Property(property.Name)
                        .IsConcurrencyToken().HasConversion(new ValueConverter<RowVersion, int>(
                            v => v.VersionNumber,
                            v => new RowVersion(v)));
                }
                else if (property.PropertyType == typeof(UpdateTime))
                {
                    modelBuilder.Entity(clrType)
                        .Property(property.Name)
                        .HasConversion(new ValueConverter<UpdateTime, DateTimeOffset>(
                            v => v.Value,
                            v => new UpdateTime(v)));
                }
                else if (property.PropertyType == typeof(DeletedTime))
                {
                    modelBuilder.Entity(clrType)
                        .Property(property.Name)
                        .HasConversion(new ValueConverter<DeletedTime, DateTimeOffset>(
                            v => v.Value,
                            v => new DeletedTime(v)));
                }
                else if (property.PropertyType == typeof(Deleted))
                {
                    var deletedProperties = properties.Where(p => p.PropertyType == typeof(Deleted)).ToList();
                    if (deletedProperties.Count > 1)
                        throw new InvalidOperationException(
                            $"实体 {clrType.Name} 包含多个 {nameof(Deleted)} 类型的属性。"
                            + $"冲突属性: {string.Join(", ", deletedProperties.Select(p => p.Name))}");

                    var entityParameter = Expression.Parameter(clrType, "entity");
                    var propertyAccess = Expression.Property(entityParameter, deletedProperties[0].Name);
                    var isPropertyFalseExpression =
                        Expression.Equal(propertyAccess, Expression.Constant(new Deleted()));
                    var filterLambda = Expression.Lambda(isPropertyFalseExpression, entityParameter);
                    modelBuilder.Entity(clrType).HasQueryFilter(filterLambda);

                    modelBuilder.Entity(clrType)
                        .Property(property.Name)
                        .HasConversion(new ValueConverter<Deleted, bool>(
                            v => v.Value,
                            v => new Deleted(v)));
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureNetCorePalTypes(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ConfigureStronglyTypedIdValueConverter(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    #region IUnitOfWork
    public IDbContextTransaction? CurrentTransaction { get; set; }

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }


    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
            CurrentTransaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
            CurrentTransaction = null;
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            CurrentTransaction = await this.BeginTransactionAsync(cancellationToken);
            await using (CurrentTransaction)
            {
                try
                {
                    // ensure field 'Id' initialized when new entity added
                    await SaveChangesAsync(cancellationToken);
                    await _mediator.DispatchDomainEventsAsync(this, 0, cancellationToken);
                    await CommitAsync(cancellationToken);
                    return true;
                }
                catch
                {
                    await RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
        else
        {
            await SaveChangesAsync(cancellationToken);
            await _mediator.DispatchDomainEventsAsync(this, 0, cancellationToken);
            return true;
        }
    }

    #endregion

    #region SaveChangesAsync

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changeTracker"></param>
    protected virtual void UpdateNetCorePalTypesBeforeSaveChanges(ChangeTracker changeTracker)
    {
        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State != EntityState.Modified) continue;

            var softDeleted = false;
            foreach (var p in entry.Properties)
            {
                if (p.Metadata.ClrType == typeof(Deleted) && p.IsModified && ((Deleted)p.CurrentValue!).Value)
                    softDeleted = true;

                if (p.IsModified) continue;
                if (p.Metadata.ClrType == typeof(RowVersion))
                {
                    var newValue = p.OriginalValue == null
                        ? new RowVersion()
                        : new RowVersion(((RowVersion)p.OriginalValue).VersionNumber + 1);
                    p.CurrentValue = newValue;
                }
                else if (p.Metadata.ClrType == typeof(UpdateTime))
                {
                    p.CurrentValue = new UpdateTime(DateTimeOffset.UtcNow);
                }
                else if (p.Metadata.ClrType == typeof(DeletedTime) && softDeleted)
                {
                    p.CurrentValue = new DeletedTime(DateTimeOffset.UtcNow);
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new CancellationToken())
    {
        UpdateNetCorePalTypesBeforeSaveChanges(ChangeTracker);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateNetCorePalTypesBeforeSaveChanges(ChangeTracker);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    #endregion
}