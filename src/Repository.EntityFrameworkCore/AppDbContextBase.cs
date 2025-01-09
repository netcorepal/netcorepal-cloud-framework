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

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
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

        protected virtual void ConfigureRowVersion(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            foreach (var clrType in modelBuilder.Model.GetEntityTypes().Select(p => p.ClrType))
            {
                var properties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType == typeof(RowVersion));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(clrType)
                        .Property(property.Name)
                        .IsConcurrencyToken().HasConversion(new ValueConverter<RowVersion, int>(
                            v => v.VersionNumber,
                            v => new RowVersion(v)));
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureRowVersion(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ConfigureStronglyTypedIdValueConverter(configurationBuilder);
            base.ConfigureConventions(configurationBuilder);
        }

        #region IUnitOfWork

        public IDbContextTransaction? CurrentTransaction { get; private set; }

        public IDbContextTransaction BeginTransaction()
        {
            if (_publisherTransactionFactory != null)
            {
                CurrentTransaction = _publisherTransactionFactory.BeginTransaction(this);
            }
            else
            {
                CurrentTransaction = Database.BeginTransaction();
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
                CurrentTransaction = this.BeginTransaction();
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
        protected virtual void UpdateRowVersionBeforeSaveChanges(ChangeTracker changeTracker)
        {
            foreach (var entry in changeTracker.Entries())
            {
                if (entry.State == EntityState.Modified)
                {
                    foreach (var p in entry.Properties)
                    {
                        if (p.Metadata.ClrType == typeof(RowVersion) && !p.IsModified)
                        {
                            var newValue = p.OriginalValue == null
                                ? new RowVersion(0)
                                : new RowVersion(((RowVersion)p.OriginalValue).VersionNumber + 1);
                            p.CurrentValue = newValue;
                        }
                    }
                }
            }
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            UpdateRowVersionBeforeSaveChanges(ChangeTracker);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
}