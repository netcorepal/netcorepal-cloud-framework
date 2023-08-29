using NetCorePal.Extensions.Repository.EntityframeworkCore.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.EventBus;
using NetCorePal.Extensions.Repository;
using System.Threading;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore
{
    public abstract class EFContext : DbContext, IEFCoreUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly IEventPublisher? _publisher;
        private IDbContextTransaction? currentTransaction = null;
        IPublisherTransactionHandler? _publisherTransactionFactory;

        protected EFContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options)
        {
            _mediator = mediator;
            _publisher = provider.GetService<IEventPublisher>();
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();

        }

        public IDbContextTransaction BeginTransaction()
        {
            if (_publisher != null && _publisherTransactionFactory != null)
            {
                currentTransaction = _publisherTransactionFactory.BeginTransaction(this);
            }
            else
            {
                currentTransaction = Database.BeginTransaction();
            }
            return currentTransaction;
        }

        #region IUnitOfWork
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            if (currentTransaction == null)
            {
                currentTransaction = this.BeginTransaction();
                using (currentTransaction)
                {
                    // ensure field 'Id' initialized when new entity added
                    await base.SaveChangesAsync(cancellationToken);
                    await _mediator.DispatchDomainEventsAsync(this);
                    await currentTransaction.CommitAsync();
                    this.currentTransaction = null;
                    return true;
                }
            }
            else
            {
                await base.SaveChangesAsync(cancellationToken);
                await _mediator.DispatchDomainEventsAsync(this);
                this.currentTransaction = null;
                return true;
            }
        }
        #endregion

    }
}
