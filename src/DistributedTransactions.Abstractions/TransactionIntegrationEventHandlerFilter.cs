using NetCorePal.Extensions.Repository;

namespace NetCorePal.Extensions.DistributedTransactions;

public class TransactionIntegrationEventHandlerFilter : IIntegrationEventHandlerFilter
{
    private readonly ITransactionUnitOfWork _unitOfWork;

    public TransactionIntegrationEventHandlerFilter(ITransactionUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task OnPublishAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next)
    {
        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            return next(context);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}