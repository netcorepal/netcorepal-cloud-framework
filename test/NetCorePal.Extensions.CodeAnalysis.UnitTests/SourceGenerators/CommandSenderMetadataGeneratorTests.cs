using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class CommandSenderMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandSenderMetadataAttribute()
    {
        // TODO: 补充具体断言
        var assembly = typeof(CommandSenderMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        Assert.Equal(3, attrs.Count);
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendWithConstraints" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendUseParams" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendWithResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithResult"}));
    }
}
