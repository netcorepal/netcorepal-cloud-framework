using NetCorePal.Extensions.Repository;

namespace NetCorePal.Extensions.DistributedTransactions;

public class TransactionIntegrationEventHandlerFilter : IIntegrationEventHandlerFilter
{
    private readonly ITransactionUnitOfWork _unitOfWork;

    public TransactionIntegrationEventHandlerFilter(ITransactionUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
    {
        await using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            await next(context);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}