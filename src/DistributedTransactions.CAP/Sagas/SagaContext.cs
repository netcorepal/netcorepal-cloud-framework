using System.Text.Json;
using MediatR;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaContext<TDbContext, TSagaData> : ISagaContext<TSagaData>
    where TDbContext : EFContext
    where TSagaData : SagaData
{
    readonly SagaRepository<TDbContext> _repository;
    private readonly IMediator _mediator;
    SagaEntity _sagaEntity = new SagaEntity("", DateTime.UtcNow.AddSeconds(30));

    public SagaContext(IMediator mediator, SagaRepository<TDbContext> repository,
        ISagaEventPublisher eventPublisher)
    {
        _mediator = mediator;
        _repository = repository;
        EventPublisher = eventPublisher;
    }

    public bool IsComplete() => _sagaEntity.IsComplete;

    public bool IsTimeout()
    {
        return _sagaEntity.WhenTimeout < DateTime.UtcNow;
    }

    public TSagaData? Data { get; set; }
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


        _sagaEntity = new SagaEntity("", DateTime.UtcNow.AddSeconds(30));
        this.Data = JsonSerializer.Deserialize<TSagaData>(_sagaEntity.SagaData)!;
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

    public async Task StartNewSagaAsync(TSagaData sagaData, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new CreateSagaCommand<TSagaData>(sagaData), cancellationToken);
        this.Data = sagaData;
        var data = JsonSerializer.Serialize(sagaData);
        var entity = new SagaEntity(data, DateTime.UtcNow.AddSeconds(30));
        await _repository.AddAsync(entity, cancellationToken);
        _sagaEntity = entity;
    }
}