using System.Text.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

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
    private SagaRepository<TDbContext> _repository;

    public CreateSagaCommandHandler(SagaRepository<TDbContext> repository)
    {
        _repository = repository;
    }


    public Task Handle(CreateSagaCommand<TSagaData> request, CancellationToken cancellationToken)
    {
        var data = JsonSerializer.Serialize(request.Data);
        var entity = new SagaEntity(data, DateTime.Now.AddSeconds(30));
        return _repository.AddAsync(entity, cancellationToken);
    }
}