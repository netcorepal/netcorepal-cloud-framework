using NetCorePal.Extensions.Repository.EntityframeworkCore.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.EventBus;
using NetCorePal.Extensions.Repository.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore
{
    public abstract class EFContext : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly IEventPublisher? _publisher;
        IPublisherTransactionHandler? _publisherTransactionFactory;

        protected EFContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options)
        {
            _mediator = mediator;
            _publisher = provider.GetService<IEventPublisher>();
            _publisherTransactionFactory = provider.GetService<IPublisherTransactionHandler>();

        }

        #region IUnitOfWork
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            IDbContextTransaction transaction;
            if (_publisher != null && _publisherTransactionFactory != null)
            {
                transaction = _publisherTransactionFactory.BeginTransaction(this);
            }
            else
            {
                transaction = await Database.BeginTransactionAsync(cancellationToken);
            }

            using (transaction)
            {
                // ensure field 'Id' initialized when new entity added
                await base.SaveChangesAsync(cancellationToken);

                await _mediator.DispatchDomainEventsAsync(this);

                await transaction.CommitAsync();
                return true;
            }
        }
        #endregion

    }
}
