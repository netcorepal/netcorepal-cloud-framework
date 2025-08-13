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
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderMethodMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderMethodMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // 现在应该有更多的属性，因为添加了新的CommandSender类
        Assert.True(attrs.Count >= 3, $"预期至少3个属性，实际生成了{attrs.Count}个");
        
        // 验证原有的CommandSender方法仍然存在
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendWithConstraints" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendUseParams" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithOutResult"}));
        Assert.Contains(attrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestCommandSender" && a.SenderMethodName == "SendWithResult" && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.RecordCommandWithResult"}));
    }
}
