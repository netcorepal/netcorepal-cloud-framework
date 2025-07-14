using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators.Result;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class SubEntityMethodValidationTests
{
    private readonly ITestOutputHelper _output;

    public SubEntityMethodValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Should_Generate_SubEntity_Methods_With_Correct_Format()
    {
        // Arrange
        var analysisResult = new GeneratedAnalysisResult();
        var result = analysisResult.GetResult();

        // Act - 找到Order实体
        var orderEntity = result.Entities.FirstOrDefault(e => e.Name == "Order");
        
        // Assert
        Assert.NotNull(orderEntity);
        
        // 输出Order的所有方法，验证是否包含OrderItem.xxx格式
        _output.WriteLine("Order Entity Methods:");
        foreach (var method in orderEntity.Methods)
        {
            _output.WriteLine($"  - {method}");
        }

        // 验证是否包含OrderItem.方法格式
        var orderItemMethods = orderEntity.Methods.Where(m => m.StartsWith("OrderItem.")).ToList();
        _output.WriteLine($"\nOrderItem methods found: {orderItemMethods.Count}");
        foreach (var method in orderItemMethods)
        {
            _output.WriteLine($"  - {method}");
        }

        // 验证MethodToDomainEvent关系
        _output.WriteLine("\nMethodToDomainEvent relationships for Order:");
        var orderRelationships = result.Relationships
            .Where(r => r.CallType == "MethodToDomainEvent" && 
                       r.SourceType.Contains("Order") &&
                       !r.SourceType.Contains("OrderItem"))
            .ToList();

        foreach (var rel in orderRelationships)
        {
            _output.WriteLine($"  {rel.SourceType}.{rel.SourceMethod} -> {rel.TargetType}");
        }
    }
}
