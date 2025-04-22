using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;

public class PropertyBuilderExtensionTests
{
    
    [Fact]
    public void UseGuidValueGenerator_ShouldThrowArgumentNullException_WhenBuilderIsNull()
    {
        // Arrange
        PropertyBuilder<TestId> builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.UseGuidValueGenerator());
    }
    
    [Fact]
    public void UseGuidValueGenerator_ShouldReturnPropertyBuilder_WhenBuilderIsNotNull()
    {
        var modelBuilder = new ModelBuilder();
        var entityType = modelBuilder.Entity<GuidTestEntity>();
        var propertyBuilder = entityType.Property(e => e.Id);
        // Act
        var result = propertyBuilder.UseGuidValueGenerator();
        // Assert
        Assert.NotNull(result);
        var v = propertyBuilder.Metadata.GetValueGeneratorFactory();
        Assert.NotNull(v);
        var s = v.Invoke(null!, null!);
        Assert.NotNull(s);
        Assert.IsType<StrongTypeGuidValueGenerator<TestId>>(s);
    }
    
#if NET9_0_OR_GREATER
    [Fact]
    public void UseGuidVersion7ValueGenerator_ShouldThrowArgumentNullException_WhenBuilderIsNull()
    {
        // Arrange
        PropertyBuilder<TestId> builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.UseGuidVersion7ValueGenerator());
    }


    [Fact]
    public void UseGuidVersion7ValueGenerator_ShouldReturnPropertyBuilder_WhenBuilderIsNotNull()
    {
        var modelBuilder = new ModelBuilder();
        var entityType = modelBuilder.Entity<GuidTestEntity>();
        var propertyBuilder = entityType.Property(e => e.Id);
        // Act
        var result = propertyBuilder.UseGuidVersion7ValueGenerator();
        // Assert
        Assert.NotNull(result);
        var v = propertyBuilder.Metadata.GetValueGeneratorFactory();
        Assert.NotNull(v);
        var s = v.Invoke(null!, null!);
        Assert.NotNull(s);
        Assert.IsType<StrongTypeGuidVersion7ValueGenerator<TestId>>(s);
    }
#endif
}


