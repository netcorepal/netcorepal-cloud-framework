using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestCommandHandlerWithOutResult : ICommandHandler<RecordCommandWithOutResult>
{
    public Task Handle(RecordCommandWithOutResult request, CancellationToken cancellationToken)
    {
        var TestAggregateRoot = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));

        var root2 = TestAggregateRoot.Create(new TestAggregateRootId(Guid.NewGuid()));

        var entity = new TestEntity(1, "Test Entity");

        var entity2 = new TestEntity2(2, "Test Entity 2");

        entity.ChangeTestEntityName("New Name");

        TestAggregateRoot.TestEntities.Add(entity2);
        // 处理逻辑
        return Task.CompletedTask;
    }
}

public class TestCommandHandlerWithResult : ICommandHandler<RecordCommandWithResult,string>
{
    public Task<string> Handle(RecordCommandWithResult request, CancellationToken cancellationToken)
    {
        // 处理逻辑
        return Task.FromResult(request.Name);
    }
}
