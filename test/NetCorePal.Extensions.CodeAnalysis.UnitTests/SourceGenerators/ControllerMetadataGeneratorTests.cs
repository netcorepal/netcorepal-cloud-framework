using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class ControllerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_ControllerMetadataAttribute()
    {
        var assembly = typeof(ControllerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMethodMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMethodMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // 断言与实际生成的 ControllerMethodMetadataAttribute 保持一致
        Assert.Equal(6, attrs.Count);
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestController" && a.ControllerMethodName == "SendRecordCommandWithResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithResult"}));
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestController" && a.ControllerMethodName == "SendRecordCommandWithOutResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestController" && a.ControllerMethodName == "MethodWithOutCommand" && a.CommandTypes.Length == 0);
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestWithPrimaryConstructorsController" && a.ControllerMethodName == "SendClassCommandWithResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.ClassCommandWithResult"}));
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestWithPrimaryConstructorsController" && a.ControllerMethodName == "SendClassCommandWithOutResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.ClassCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestWithPrimaryConstructorsController" && a.ControllerMethodName == "SendMultiCommand" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.ClassCommandWithOutResult","NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithResult"}));
    }
}
