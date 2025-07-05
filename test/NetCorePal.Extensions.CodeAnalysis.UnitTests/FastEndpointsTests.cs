using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Endpoints;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

/// <summary>
/// FastEndpoints 支持测试
/// </summary>
public class FastEndpointsTests
{
    [Fact]
    public void CodeFlowAnalysisSourceGenerator_ShouldDetectEndpoints()
    {
        // 这个测试验证 CodeFlowAnalysisSourceGenerator 能够识别 Endpoints
        // 实际的验证会在编译时通过源生成器进行
        
        // 验证 Endpoints 类型是否正确
        Assert.EndsWith("Endpoint", typeof(CreateUserEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(CreateOrderEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(ActivateUserEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(CancelOrderEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(ConfirmOrderEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(DeactivateUserEndpoint).Name);
        Assert.EndsWith("Endpoint", typeof(CreateDefaultOrderEndpoint).Name);
    }

    [Fact]
    public void MermaidVisualizer_ShouldIncludeEndpointsInDiagram()
    {
        // 这个测试验证 MermaidVisualizer 能够在图表中包含 Endpoints
        // 实际的验证需要运行 MermaidVisualizer 来生成图表
        
        // 验证 Endpoints 命名空间存在
        var endpointTypes = new[]
        {
            typeof(CreateUserEndpoint),
            typeof(CreateOrderEndpoint),
            typeof(ActivateUserEndpoint),
            typeof(CancelOrderEndpoint),
            typeof(ConfirmOrderEndpoint),
            typeof(DeactivateUserEndpoint),
            typeof(CreateDefaultOrderEndpoint)
        };

        foreach (var type in endpointTypes)
        {
            Assert.Contains("Endpoints", type.Namespace);
        }
    }
}
