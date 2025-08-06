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