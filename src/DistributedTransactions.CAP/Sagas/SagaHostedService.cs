using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaHostedService<TDbContext> : BackgroundService
    where TDbContext : AppDbContextBase
{
    private readonly SagaRepository<TDbContext> _sagaRepository;
    private readonly ISagaManager _sagaManager;

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