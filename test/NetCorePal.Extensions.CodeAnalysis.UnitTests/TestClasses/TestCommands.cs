using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

public record EndpointCommandWithOutResult(string Name) : ICommand;

public record EndpointCommandWithResult(string Name) : ICommand<string>;

public record RecordCommandWithOutResult(string name) : ICommand;

public record RecordCommandWithResult(string Name) : ICommand<string>;


public class ClassCommandWithOutResult : ICommand
{
    public string Name { get; set; } = string.Empty;
}

public class ClassCommandWithResult : ICommand<string>
{
    public string Name { get; set; } = string.Empty;
}

public record TestAggregateRootNameChangedDomainEventHandlerCommand1() : ICommand;
public record TestAggregateRootNameChangedDomainEventHandlerCommand2() : ICommand<string>;


public record TestIntegrationEventCommand(string Name) : ICommand;

public record TestIntegrationEventCommand2(string Name) : ICommand<string>;

// 新增的测试命令
public record TestGenericCommand<T>(T Data) : ICommand<T> where T : class;

public record TestBatchCommand(string[] Items) : ICommand;

public record TestStreamCommand(int Index, string Data) : ICommand<string>;

public record TestValidationCommand(string Name, int Age) : ICommand<string>;

public record TestErrorHandlingCommand(string Action) : ICommand<string>;

public record TestParallelCommand(string Id, string Data) : ICommand;

public record TestConditionalCommand(string Type, string Data) : ICommand<string>;

public class TestComplexCommand : ICommand<string>
{
    public string Operation { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}