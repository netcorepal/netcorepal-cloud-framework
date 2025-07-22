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
    }
}
