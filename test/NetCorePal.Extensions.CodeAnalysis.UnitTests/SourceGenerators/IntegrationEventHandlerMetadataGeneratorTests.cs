using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class IntegrationEventHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_IntegrationEventHandlerMetadataAttribute()
    {
        var assembly = typeof(IntegrationEventHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 目录下所有集成事件处理器及其事件类型
        var expected = new[]
        {
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedIntegrationEventHandler", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedIntegrationEvent", new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand"}),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEventHandler", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEvent", new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand"}),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEventHandler2", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEvent2", new string[0]),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEventHandler3", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEvent2", new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand","NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestIntegrationEventCommand2"})
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var (handler, evt, commands) in expected)
        {
            Assert.Contains(attrs, a => a.HandlerType == handler && a.EventType == evt && a.CommandTypes.SequenceEqual(commands));
        }
    }
}
