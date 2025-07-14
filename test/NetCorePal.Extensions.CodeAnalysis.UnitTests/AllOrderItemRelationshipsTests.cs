using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators.Result;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class AllOrderItemRelationshipsTests
{
    private readonly ITestOutputHelper _output;

    public AllOrderItemRelationshipsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_Generate_All_OrderItem_MethodToDomainEvent_Relationships()
    {
        // Arrange & Act
        var analysisResult = new GeneratedAnalysisResult();
        var result = analysisResult.GetResult();

        // Assert - 查找所有OrderItem相关的MethodToDomainEvent关系
        var orderItemRelationships = result.Relationships
            .Where(r => r.CallType == "MethodToDomainEvent" && 
                       r.SourceType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
                       r.SourceMethod.StartsWith("OrderItem."))
            .ToList();

        _output.WriteLine("All OrderItem MethodToDomainEvent Relationships:");
        foreach (var rel in orderItemRelationships)
        {
            _output.WriteLine($"✅ new CallRelationship(\"{rel.SourceType}\", \"{rel.SourceMethod}\", \"{rel.TargetType}\", \"{rel.TargetMethod}\", \"{rel.CallType}\")");
        }

        // 验证期望的关系
        var expectedMethods = new[]
        {
            "OrderItem.ctor",
            "OrderItem.UpdateQuantity", 
            "OrderItem.UpdateUnitPrice",
            "OrderItem.Remove"
        };

        foreach (var expectedMethod in expectedMethods)
        {
            var found = orderItemRelationships.Any(r => r.SourceMethod == expectedMethod);
            Assert.True(found, $"Missing relationship for method: {expectedMethod}");
            _output.WriteLine($"✓ Found relationship for {expectedMethod}");
        }

        _output.WriteLine($"\nTotal OrderItem relationships found: {orderItemRelationships.Count}");
        Assert.True(orderItemRelationships.Count >= 4, "Should have at least 4 OrderItem method relationships");
    }
}
