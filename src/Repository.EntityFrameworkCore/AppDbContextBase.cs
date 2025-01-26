using System.Diagnostics;
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
    readonly IPublisherTransactionHandler? _publisherTransactionFactory;

    protected AppDbContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) :
        base(options)
    {
        _mediator = mediator;
        _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();
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

    public IDbContextTransaction? CurrentTransaction { get; private set; }

    public async ValueTask<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_publisherTransactionFactory != null)
        {
            CurrentTransaction = await _publisherTransactionFactory.BeginTransactionAsync(this);
        }
        else
        {
            CurrentTransaction = await Database.BeginTransactionAsync();
        }

        WriteTransactionBegin(new TransactionBegin(CurrentTransaction.TransactionId));
        return CurrentTransaction;
    }


    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
            WriteTransactionCommit(new TransactionCommit(CurrentTransaction.TransactionId));
            CurrentTransaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
            WriteTransactionRollback(new TransactionRollback(CurrentTransaction.TransactionId));
            CurrentTransaction = null;
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            CurrentTransaction = await this.BeginTransactionAsync();
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
            if (entry.State == EntityState.Modified)
            {
                foreach (var p in entry.Properties)
                {
                    if (p.IsModified) continue;
                    if (p.Metadata.ClrType == typeof(RowVersion))
                    {
                        var newValue = p.OriginalValue == null
                            ? new RowVersion(0)
                            : new RowVersion(((RowVersion)p.OriginalValue).VersionNumber + 1);
                        p.CurrentValue = newValue;
                    }
                    else if (p.Metadata.ClrType == typeof(UpdateTime))
                    {
                        p.CurrentValue = new UpdateTime(DateTimeOffset.UtcNow);
                    }
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


    #region DiagnosticListener

    void WriteTransactionBegin(TransactionBegin data)
    {
        if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.TransactionBegin))
        {
            _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.TransactionBegin, data);
        }
    }

    void WriteTransactionCommit(TransactionCommit data)
    {
        if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.TransactionCommit))
        {
            _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.TransactionCommit, data);
        }
    }

    void WriteTransactionRollback(TransactionRollback data)
    {
        if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.TransactionRollback))
        {
            _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.TransactionRollback, data);
        }
    }

    #endregion
}