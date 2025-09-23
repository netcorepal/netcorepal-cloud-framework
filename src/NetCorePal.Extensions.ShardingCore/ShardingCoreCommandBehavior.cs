using MediatR;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public class ShardingCoreCommandBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    where TCommand : IBaseCommand
{
    private static readonly PublishedMessageDataSourceContext ShardingDatabaseContext =
        new PublishedMessageDataSourceContext();

    public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ShardingDatabaseContext.Init();
        var r = await next(cancellationToken);
        ShardingDatabaseContext.Clear();
        return r;
    }
}