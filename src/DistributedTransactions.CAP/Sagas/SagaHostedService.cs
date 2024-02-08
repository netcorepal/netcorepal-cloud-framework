using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaHostedService<TDbContext> : BackgroundService
    where TDbContext : AppDbContextBase
{
#pragma warning disable S4487
    private readonly SagaRepository<TDbContext> _sagaRepository;
#pragma warning restore S4487
#pragma warning disable S4487
    private readonly ISagaManager _sagaManager;
#pragma warning restore S4487

    public SagaHostedService(SagaRepository<TDbContext> sagaRepository, ISagaManager sagaManager)
    {
        _sagaRepository = sagaRepository;
        _sagaManager = sagaManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}