using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class CommandMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandMetadataAttribute()
    {
        var assembly = typeof(CommandMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 中所有命令类型（根据自动生成的元数据调整）
        var expected = new[]
        {
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.EndpointCommandWithOutResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.EndpointCommandWithResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.ClassCommandWithOutResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.ClassCommandWithResult",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEventHandlerCommand1",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEventHandlerCommand2",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand2"
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var type in expected)
        {
            Assert.Contains(attrs, a => a.CommandType == type);
        }
    }
}
