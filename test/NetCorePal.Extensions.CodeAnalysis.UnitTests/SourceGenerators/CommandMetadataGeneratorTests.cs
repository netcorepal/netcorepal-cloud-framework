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
        // TestClasses 中所有命令类型（根据自动生成的元数据调整），现在包括新添加的命令
        var originalExpected = new[]
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
        
        // 验证至少包含原有的命令，现在可能有更多新的命令
        Assert.True(attrs.Count >= originalExpected.Length, $"预期至少{originalExpected.Length}个属性，实际生成了{attrs.Count}个");
        
        // 验证所有原有的命令仍然存在
        foreach (var type in originalExpected)
        {
            Assert.Contains(attrs, a => a.CommandType == type);
        }
    }
}
