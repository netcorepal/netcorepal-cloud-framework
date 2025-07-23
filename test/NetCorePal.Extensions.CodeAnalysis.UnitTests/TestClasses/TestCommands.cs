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