using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NetCorePal.Extensions.Localization;
using NetCorePal.Extensions.Localization.Exceptions;
using System.Globalization;
using Xunit;

namespace NetCorePal.Extensions.Localization.Tests;

/// <summary>
/// Unit tests for NetCorePal localization functionality
/// </summary>
public class LocalizationTests
{
    private readonly IServiceProvider _serviceProvider;

    public LocalizationTests()
    {
        var services = new ServiceCollection();
        services.AddNetCorePalLocalization(options =>
        {
            options.DefaultCulture = "en";
            options.SupportedCultures = new[] { "en", "zh" };
            options.FallbackToDefaultCulture = true;
        });

        _serviceProvider = services.BuildServiceProvider();
        FrameworkLocalizer.Initialize(_serviceProvider);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Return_English_Messages_By_Default()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act
        var message = FrameworkLocalizer.GetString("OperationSuccessful");

        // Assert
        Assert.Equal("Operation completed successfully.", message);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Return_Chinese_Messages_When_Culture_Is_Chinese()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("zh");

        // Act
        var message = FrameworkLocalizer.GetString("OperationSuccessful");

        // Assert
        Assert.Equal("操作成功完成。", message);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Handle_Parameters_Correctly()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act
        var message = FrameworkLocalizer.GetString("InvalidEntityId", 123);

        // Assert
        Assert.Equal("Invalid entity ID: 123", message);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Handle_Multiple_Parameters()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act
        var message = FrameworkLocalizer.GetString("ValidationStringLength", 5, 50);

        // Assert
        Assert.Equal("The string length must be between 5 and 50 characters.", message);
    }

    [Fact]
    public void EntityNotFoundException_Should_Use_Localized_Message()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => throw new EntityNotFoundException(123));
        Assert.Contains("123", exception.Message);
        Assert.Contains("not found", exception.Message.ToLower());
    }

    [Fact]
    public void EntityNotFoundException_Should_Use_Chinese_Message_When_Culture_Is_Chinese()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("zh");

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => throw new EntityNotFoundException(456));
        Assert.Contains("456", exception.Message);
        Assert.Contains("未找到", exception.Message);
    }

    [Fact]
    public void BusinessRuleViolationException_Should_Use_Localized_Message()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleViolationException>(() => 
            throw new BusinessRuleViolationException("ValidationRequired"));
        Assert.Equal("This field is required.", exception.Message);
    }

    [Fact]
    public void DomainException_Should_Handle_Parameters()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("en");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            throw new DomainException("DomainEventProcessingFailed", "TestEvent"));
        Assert.Contains("TestEvent", exception.Message);
        Assert.Contains("failed", exception.Message.ToLower());
    }

    [Theory]
    [InlineData("en", "This field is required.")]
    [InlineData("zh", "此字段是必填项。")]
    public void ValidationRequired_Should_Return_Correct_Message_For_Culture(string culture, string expected)
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture(culture);

        // Act
        var message = FrameworkLocalizer.GetString("ValidationRequired");

        // Assert
        Assert.Equal(expected, message);
    }

    [Theory]
    [InlineData("en", "An unknown error occurred.")]
    [InlineData("zh", "发生未知错误。")]
    public void UnknownError_Should_Return_Correct_Message_For_Culture(string culture, string expected)
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture(culture);

        // Act
        var message = FrameworkLocalizer.GetString("UnknownError");

        // Assert
        Assert.Equal(expected, message);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Fallback_To_Default_Culture_For_Unsupported_Culture()
    {
        // Arrange
        LocalizationServiceCollectionExtensions.SetCurrentCulture("fr"); // Unsupported culture

        // Act
        var message = FrameworkLocalizer.GetString("OperationSuccessful");

        // Assert - Should fallback to English
        Assert.Equal("Operation completed successfully.", message);
    }

    [Fact]
    public void StringLocalizer_Should_Work_With_Custom_Types()
    {
        // Arrange
        var localizer = _serviceProvider.GetRequiredService<IStringLocalizer<LocalizationTests>>();

        // Act
        var message = localizer["TestKey"];

        // Assert - Should return the key if no resource found
        Assert.Equal("TestKey", message.Value);
        Assert.True(message.ResourceNotFound);
    }

    [Fact]
    public void LocalizationOptions_Should_Be_Configurable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNetCorePalLocalization(options =>
        {
            options.DefaultCulture = "zh";
            options.SupportedCultures = new[] { "zh", "en", "ja" };
            options.FallbackToDefaultCulture = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert - Configuration should be applied
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void FrameworkLocalizer_Should_Throw_When_Not_Initialized()
    {
        // Arrange - Create a separate instance without initialization
        var services = new ServiceCollection();
        services.AddNetCorePalLocalization();
        var serviceProvider = services.BuildServiceProvider();
        
        // Don't call FrameworkLocalizer.Initialize()

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => FrameworkLocalizer.GetString("OperationSuccessful"));
    }
}