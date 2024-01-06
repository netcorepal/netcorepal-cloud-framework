using System.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
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

        readonly string _name;

        protected AppDbContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) :
            base(options)
        {
            _name = GetType().FullName ?? GetType().Name;
            _mediator = mediator;
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();
        }


        protected virtual void ConfigureStronglyTypedIdValueConverter(ModelConfigurationBuilder configurationBuilder)
        {
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
            Guid id = Guid.NewGuid();
            if (CurrentTransaction == null)
            {
                CurrentTransaction = this.BeginTransaction();
                await using (CurrentTransaction)
                {
                    try
                    {
                        // ensure field 'Id' initialized when new entity added
                        await base.SaveChangesAsync(cancellationToken);
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
                await base.SaveChangesAsync(cancellationToken);
                await _mediator.DispatchDomainEventsAsync(this, 0, cancellationToken);
                return true;
            }
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