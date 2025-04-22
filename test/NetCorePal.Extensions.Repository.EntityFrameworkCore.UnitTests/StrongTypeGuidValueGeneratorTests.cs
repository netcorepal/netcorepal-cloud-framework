using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;

public class StrongTypeGuidValueGeneratorTests
{
    [Fact]
    public void Next_ShouldReturnNewGuid()
    {
        // Arrange
        var generator = new StrongTypeGuidValueGenerator<TestId>();

        // Act
        var result = generator.Next(null!);
        
        var result2 = generator.Next(null!);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result2);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(result.Id, result2.Id);
    }
}

public partial record TestId : IGuidStronglyTypedId;