using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaHostedService<TDbContext> : BackgroundService
    where TDbContext : EFContext
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}