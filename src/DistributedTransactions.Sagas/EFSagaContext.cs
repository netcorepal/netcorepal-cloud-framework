using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class EFSagaContext<TDbContext, TSagaData> : ISagaContext<TSagaData>
    where TDbContext : EFContext
    where TSagaData : SagaData
{
    readonly SagaRepository<TDbContext> _repository;
    SagaEntity _sagaEntity;

    public EFSagaContext(SagaRepository<TDbContext> repository, SagaEntity sagaEntity, TSagaData data,
        ISagaEventPublisher eventPublisher)
    {
        _repository = repository;
        Data = data;
        EventPublisher = eventPublisher;
        _sagaEntity = sagaEntity;
    }

    public bool IsComplete() => _sagaEntity.IsComplete;

    public bool IsTimeout()
    {
        return _sagaEntity.WhenTimeout < DateTime.Now;
    }

    public TSagaData Data { get; }
    public ISagaEventPublisher EventPublisher { get; }

    public void MarkAsComplete()
    {
        _sagaEntity.IsComplete = true;
        _repository.Update(_sagaEntity);
    }

    public async Task InitAsync(Guid? sagaId, CancellationToken cancellationToken = default)
    {
        if (sagaId.HasValue)
        {
            var saga = await _repository.GetAsync(sagaId.Value, cancellationToken);
            if (saga != null)
            {
                _sagaEntity = saga;
                return;
            }
        }
        _sagaEntity = new SagaEntity("", DateTime.Now.AddSeconds(30));
        await _repository.AddAsync(_sagaEntity, cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetAsync(_sagaEntity.Id);
        if (data != null)
        {
            _sagaEntity = data;
        }
    }

    public void SetCurrentEvent(string eventName)
    {
    }
}