using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators.Result;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class CommandToAggregateMethodValidationTests
{
    private readonly ITestOutputHelper _output;

    public CommandToAggregateMethodValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_Generate_Correct_CommandToAggregateMethod_For_SubEntity_Constructor()
    {
        // Arrange & Act
        var analysisResult = new GeneratedAnalysisResult();
        var result = analysisResult.GetResult();

        // Assert - 查找AddOrderItemCommand的CommandToAggregateMethod关系
        var addOrderItemCommandRelationships = result.Relationships
            .Where(r => r.CallType == "CommandToAggregateMethod" && 
                       r.SourceType.Contains("AddOrderItemCommand"))
            .ToList();

        _output.WriteLine("AddOrderItemCommand CommandToAggregateMethod Relationships:");
        foreach (var rel in addOrderItemCommandRelationships)
        {
            _output.WriteLine($"✅ new CallRelationship(\"{rel.SourceType}\", \"{rel.SourceMethod}\", \"{rel.TargetType}\", \"{rel.TargetMethod}\", \"{rel.CallType}\")");
        }

        // 检查是否存在OrderItem相关的关系
        var orderItemRelationships = addOrderItemCommandRelationships
            .Where(r => r.TargetType.Contains("OrderItem") || r.TargetMethod.Contains("OrderItem"))
            .ToList();

        _output.WriteLine($"\nOrderItem related relationships ({orderItemRelationships.Count}):");
        foreach (var rel in orderItemRelationships)
        {
            _output.WriteLine($"  Target: {rel.TargetType}");
            _output.WriteLine($"  Method: {rel.TargetMethod}");
            _output.WriteLine($"  Expected: Target should be Order, Method should be OrderItem.ctor");
            _output.WriteLine($"  ---");
        }

        // 验证是否存在期望的关系
        var expectedRelationship = addOrderItemCommandRelationships.FirstOrDefault(r =>
            r.SourceType.Contains("AddOrderItemCommand") &&
            r.SourceMethod == "Handle" &&
            r.TargetType.Contains("Order") &&
            !r.TargetType.Contains("OrderItem") && // 目标类型不应该是OrderItem
            r.TargetMethod == "OrderItem.ctor");

        Assert.NotNull(expectedRelationship);
        _output.WriteLine($"\n✅ Found expected CommandToAggregateMethod relationship:");
        _output.WriteLine($"   Source: {expectedRelationship.SourceType}");
        _output.WriteLine($"   Method: {expectedRelationship.SourceMethod}");
        _output.WriteLine($"   Target: {expectedRelationship.TargetType}");
        _output.WriteLine($"   TargetMethod: {expectedRelationship.TargetMethod}");
        _output.WriteLine($"   CallType: {expectedRelationship.CallType}");
    }
}
