using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class CommandHandlerEntityMethodMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandHandlerMetadataAttribute()
    {
        // TODO: 补充具体断言
        var assembly = typeof(CommandHandlerEntityMethodMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandHandlerEntityMethodMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandHandlerEntityMethodMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        Assert.NotEmpty(attrs);
        // 断言数量
        Assert.Equal(5, attrs.Count);
        // 断言具体内容
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandHandlerWithOutResult" && a.CommandType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult" && a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot" && a.EntityMethodName == "Create");
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandHandlerWithOutResult" && a.CommandType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult" && a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity" && a.EntityMethodName == "ChangeTestEntityName");
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandHandlerWithOutResult" && a.CommandType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult" && a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot" && a.EntityMethodName == ".ctor");
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandHandlerWithOutResult" && a.CommandType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult" && a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity" && a.EntityMethodName == ".ctor");
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandHandlerWithOutResult" && a.CommandType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult" && a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity2" && a.EntityMethodName == ".ctor");
    }
}
