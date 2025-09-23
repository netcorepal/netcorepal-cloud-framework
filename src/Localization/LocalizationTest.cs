using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.Localization;
using System.Globalization;

namespace NetCorePal.Extensions.Localization.Tests;

/// <summary>
/// Simple console test to verify localization functionality
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("NetCorePal Localization Test");
        Console.WriteLine("============================");

        // Setup services
        var services = new ServiceCollection();
        services.AddNetCorePalLocalization(options =>
        {
            options.DefaultCulture = "en";
            options.SupportedCultures = new[] { "en", "zh" };
            options.FallbackToDefaultCulture = true;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Initialize framework localizer
        FrameworkLocalizer.Initialize(serviceProvider);

        // Test English (default)
        Console.WriteLine("\n--- Testing English (Default) ---");
        SetCurrentCulture("en");
        TestFrameworkMessages();

        // Test Chinese
        Console.WriteLine("\n--- Testing Chinese ---");
        SetCurrentCulture("zh");
        TestFrameworkMessages();

        // Test fallback behavior
        Console.WriteLine("\n--- Testing Fallback Behavior ---");
        SetCurrentCulture("fr"); // Unsupported culture
        TestFrameworkMessages();

        Console.WriteLine("\n--- Testing Localized Exceptions ---");
        TestLocalizedExceptions();

        Console.WriteLine("\nLocalization test completed successfully!");
    }

    private static void SetCurrentCulture(string culture)
    {
        LocalizationServiceCollectionExtensions.SetCurrentCulture(culture);
        Console.WriteLine($"Current Culture: {CultureInfo.CurrentUICulture.Name}");
    }

    private static void TestFrameworkMessages()
    {
        var messages = new Dictionary<string, Func<string>>
        {
            ["ValidationRequired"] = () => FrameworkLocalizer.GetString("ValidationRequired"),
            ["ValidationInvalidFormat"] = () => FrameworkLocalizer.GetString("ValidationInvalidFormat"),
            ["ValidationStringLength"] = () => FrameworkLocalizer.GetString("ValidationStringLength", 5, 50),
            ["OperationSuccessful"] = () => FrameworkLocalizer.GetString("OperationSuccessful"),
            ["OperationFailed"] = () => FrameworkLocalizer.GetString("OperationFailed"),
            ["NotFound"] = () => FrameworkLocalizer.GetString("NotFound"),
            ["InvalidEntityId"] = () => FrameworkLocalizer.GetString("InvalidEntityId", 123),
            ["AggregateNotFound"] = () => FrameworkLocalizer.GetString("AggregateNotFound", 456),
            ["DomainEventProcessingFailed"] = () => FrameworkLocalizer.GetString("DomainEventProcessingFailed", "TestEvent"),
            ["TransactionRolledBack"] = () => FrameworkLocalizer.GetString("TransactionRolledBack"),
            ["ConcurrencyConflict"] = () => FrameworkLocalizer.GetString("ConcurrencyConflict")
        };

        foreach (var (key, getMessage) in messages)
        {
            try
            {
                var message = getMessage();
                Console.WriteLine($"  {key}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {key}: ERROR - {ex.Message}");
            }
        }
    }

    private static void TestLocalizedExceptions()
    {
        try
        {
            SetCurrentCulture("en");
            throw new EntityNotFoundException(123);
        }
        catch (EntityNotFoundException ex)
        {
            Console.WriteLine($"English EntityNotFoundException: {ex.Message}");
        }

        try
        {
            SetCurrentCulture("zh");
            throw new EntityNotFoundException(456);
        }
        catch (EntityNotFoundException ex)
        {
            Console.WriteLine($"Chinese EntityNotFoundException: {ex.Message}");
        }

        try
        {
            SetCurrentCulture("en");
            throw new BusinessRuleViolationException("ValidationRequired");
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"English BusinessRuleViolationException: {ex.Message}");
        }

        try
        {
            SetCurrentCulture("zh");
            throw new BusinessRuleViolationException("ValidationRequired");
        }
        catch (BusinessRuleViolationException ex)
        {
            Console.WriteLine($"Chinese BusinessRuleViolationException: {ex.Message}");
        }
    }
}