using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    public abstract class AppDbContextBase : DbContext, ITransactionUnitOfWork
    {
        private readonly IMediator _mediator;
        readonly IPublisherTransactionHandler? _publisherTransactionFactory;

        protected AppDbContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options)
        {
            _mediator = mediator;
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();
        }


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
            return CurrentTransaction;
        }


        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.CommitAsync(cancellationToken);
                CurrentTransaction = null;
            }
        }

        #region IUnitOfWork
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentTransaction == null)
            {
                CurrentTransaction = this.BeginTransaction();
                using (CurrentTransaction)
                {
                    // ensure field 'Id' initialized when new entity added
                    await base.SaveChangesAsync(cancellationToken);
                    await _mediator.DispatchDomainEventsAsync(this, 0, cancellationToken);
                    await CommitAsync(cancellationToken);
                    return true;
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

    }
}
