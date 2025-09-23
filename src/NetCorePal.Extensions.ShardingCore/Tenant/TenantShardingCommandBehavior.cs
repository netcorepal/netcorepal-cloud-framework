using MediatR;
using NetCorePal.Context;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Tenant;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public class TenantShardingCommandBehavior<TCommand, TResponse>(
    ITenantDataSourceProvider provider,
    IContextAccessor contextAccessor) : IPipelineBehavior<TCommand, TResponse>
    where TCommand : IBaseCommand
{
    private static readonly PublishedMessageDataSourceContext ShardingDatabaseContext =
        new PublishedMessageDataSourceContext();

    public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ShardingDatabaseContext.Init();
        var context = contextAccessor.GetContext<TenantContext>();
        if (context?.TenantId != null)
        {
            var dataSourceName = provider.GetDataSourceName(context.TenantId);
            if (!string.IsNullOrEmpty(dataSourceName))
            {
                ShardingDatabaseContext.SetDataSourceName(dataSourceName);
            }
        }
        else
        {
            throw new Exception("aaa");
        }
        var r = await next(cancellationToken);
        ShardingDatabaseContext.Clear();
        return r;
    }
}