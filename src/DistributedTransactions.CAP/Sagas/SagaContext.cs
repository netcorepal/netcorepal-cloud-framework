using System.Text.Json;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaContext<TDbContext, TSagaData> : ISagaContext<TSagaData>
    where TDbContext : AppDbContextBase
    where TSagaData : SagaData
{
    readonly SagaRepository<TDbContext> _repository;
    SagaEntity _sagaEntity = new(Guid.Empty, "", DateTime.UtcNow.AddSeconds(30));

    public SagaContext(SagaRepository<TDbContext> repository,
        ISagaEventPublisher eventPublisher)
    {
        _repository = repository;
        EventPublisher = eventPublisher;
    }

    public bool IsComplete() => _sagaEntity.IsComplete;

    public bool IsTimeout()
    {
        return _sagaEntity.WhenTimeout < DateTime.UtcNow;
    }

    TSagaData? _sagaData;

    public TSagaData Data
    {
        get
        {
            return _sagaData ?? throw new Exception("SagaData is null");
        }
    }
    public ISagaEventPublisher EventPublisher { get; }

    public void MarkAsComplete()
    {
        _sagaEntity.IsComplete = true;
        _sagaEntity.SagaData = JsonSerializer.Serialize(Data);
        _repository.Update(_sagaEntity);
    }

    public async Task InitAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _repository.GetAsync(sagaId, cancellationToken);
        if (saga != null)
        {
            _sagaEntity = saga;
            _sagaData = JsonSerializer.Deserialize<TSagaData>(saga.SagaData);
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var data = await _repository.NoCacheGetAsync(_sagaEntity.Id, cancellationToken);
        if (data != null)
        {
            _sagaEntity = data;
        }
    }

    public async Task StartNewSagaAsync(TSagaData sagaData, CancellationToken cancellationToken = default)
    {
        _sagaData = sagaData;
        var data = JsonSerializer.Serialize(sagaData);
        var entity = new SagaEntity(sagaData.SagaId, data, DateTime.UtcNow.AddSeconds(30));
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        _sagaEntity = entity;
    }
}