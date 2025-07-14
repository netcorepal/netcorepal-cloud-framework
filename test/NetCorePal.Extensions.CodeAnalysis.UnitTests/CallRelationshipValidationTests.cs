using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators.Result;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class CallRelationshipValidationTests
{
    private readonly ITestOutputHelper _output;

    public CallRelationshipValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_Generate_Correct_OrderItem_Constructor_Relationship()
    {
        // Arrange & Act
        var analysisResult = new GeneratedAnalysisResult();
        var result = analysisResult.GetResult();

        // Assert - 查找OrderItem构造函数的MethodToDomainEvent关系
        var orderItemConstructorRelationships = result.Relationships
            .Where(r => r.CallType == "MethodToDomainEvent" && 
                       r.SourceMethod.Contains("OrderItem.ctor"))
            .ToList();

        _output.WriteLine("OrderItem Constructor MethodToDomainEvent Relationships:");
        foreach (var rel in orderItemConstructorRelationships)
        {
            _output.WriteLine($"  Source: {rel.SourceType}");
            _output.WriteLine($"  Method: {rel.SourceMethod}");
            _output.WriteLine($"  Target: {rel.TargetType}");
            _output.WriteLine($"  CallType: {rel.CallType}");
            _output.WriteLine($"  ---");
        }

        // 验证是否存在期望的关系
        var expectedRelationship = orderItemConstructorRelationships.FirstOrDefault(r =>
            r.SourceType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            r.SourceMethod == "OrderItem.ctor" &&
            r.TargetType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderItemAddedDomainEvent");

        Assert.NotNull(expectedRelationship);
        _output.WriteLine($"✅ Found expected relationship:");
        _output.WriteLine($"   new CallRelationship(\"{expectedRelationship.SourceType}\", \"{expectedRelationship.SourceMethod}\", \"{expectedRelationship.TargetType}\", \"{expectedRelationship.TargetMethod}\", \"{expectedRelationship.CallType}\")");
    }
}
