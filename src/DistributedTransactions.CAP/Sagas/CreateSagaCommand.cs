using System.Text.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class CreateSagaCommand<TSagaData> : ICommand where TSagaData : SagaData
{
    public CreateSagaCommand(TSagaData data)
    {
        Data = data;
    }

    public TSagaData Data { get; }
}

public class CreateSagaCommandHandler<TDbContext, TSagaData> : ICommandHandler<CreateSagaCommand<TSagaData>>
    where TDbContext : EFContext
    where TSagaData : SagaData
{
    readonly SagaRepository<TDbContext> _repository;

    public CreateSagaCommandHandler(SagaRepository<TDbContext> repository)
    {
        _repository = repository;
    }


    public async Task Handle(CreateSagaCommand<TSagaData> request, CancellationToken cancellationToken)
    {
        var data = JsonSerializer.Serialize(request.Data);
        var entity = new SagaEntity(request.Data.SagaId, data, DateTime.UtcNow.AddSeconds(30));
        await _repository.AddAsync(entity, cancellationToken);

    }
}