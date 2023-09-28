using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Extensions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity;

public abstract class AppIdentityUserContextBase<
    TUser, TKey, TUserClaim, TUserLogin,
    TUserToken> : IdentityUserContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken>, ITransactionUnitOfWork
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    readonly IMediator _mediator;
    readonly IPublisherTransactionHandler? _publisherTransactionFactory;

    protected AppIdentityUserContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) :
        base(options)
    {
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

    #region ITransactionUnitOfWork

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

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            CurrentTransaction = this.BeginTransaction();
            await using (CurrentTransaction)
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

public abstract class AppIdentityUserContextBase<TUser, TKey> : AppIdentityUserContextBase<
    TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>,
    IdentityUserToken<TKey>>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    protected AppIdentityUserContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) :
        base(options, mediator, provider)
    {
    }
}

public class AppIdentityUserContextBase<TUser>
    : AppIdentityUserContextBase<TUser, string>
    where TUser : IdentityUser
{
    public AppIdentityUserContextBase(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(
        options, mediator, provider)
    {
    }
}