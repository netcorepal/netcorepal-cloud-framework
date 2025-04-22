using NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;
public class StrongTypeGuidVersion7ValueGeneratorTests
{
    [Fact]
    public void Next_ShouldReturnNewGuid()
    {
        // Arrange
        var generator = new StrongTypeGuidVersion7ValueGenerator<TestId>();

        // Act
        var result = generator.Next(null!);
        
        var result2 = generator.Next(null!);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result2);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(result.Id, result2.Id);
#if NET9_0_OR_GREATER
        Assert.Equal(7, result.Id.Version);
        Assert.Equal(7, result2.Id.Version);
#endif
    }
}
