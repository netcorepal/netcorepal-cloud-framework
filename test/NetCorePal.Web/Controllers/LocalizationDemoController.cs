using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NetCorePal.Extensions.Localization;
using NetCorePal.Extensions.Localization.Exceptions;
using NetCorePal.Extensions.Localization.Validation;
using System.ComponentModel.DataAnnotations;

namespace NetCorePal.Web.Controllers;

/// <summary>
/// Demo controller to showcase internationalization features
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocalizationDemoController : ControllerBase
{
    private readonly IStringLocalizer<LocalizationDemoController> _localizer;

    public LocalizationDemoController(IStringLocalizer<LocalizationDemoController> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Get a localized greeting message
    /// </summary>
    /// <param name="name">User name</param>
    /// <returns>Localized greeting</returns>
    [HttpGet("greeting")]
    public ActionResult<string> GetGreeting([FromQuery] string name = "World")
    {
        // Example of using framework localization
        if (string.IsNullOrWhiteSpace(name))
        {
            var errorMessage = FrameworkLocalizer.GetString("ValidationRequired");
            return BadRequest(errorMessage);
        }

        var successMessage = FrameworkLocalizer.GetString("OperationSuccessful");
        return Ok($"Hello {name}! {successMessage}");
    }

    /// <summary>
    /// Demo validation with localized error messages
    /// </summary>
    /// <param name="request">User registration request</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    public ActionResult<string> Register([FromBody] UserRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(FrameworkLocalizer.GetString("OperationSuccessful"));
    }

    /// <summary>
    /// Demo exception handling with localized messages
    /// </summary>
    /// <param name="userId">User ID to find</param>
    /// <returns>User information or error</returns>
    [HttpGet("user/{userId}")]
    public ActionResult<object> GetUser(int userId)
    {
        if (userId <= 0)
        {
            throw new BusinessRuleViolationException("InvalidEntityId", userId);
        }

        if (userId == 999)
        {
            throw new EntityNotFoundException(userId);
        }

        return Ok(new { Id = userId, Name = "Test User" });
    }

    /// <summary>
    /// Get all available framework messages in current culture
    /// </summary>
    /// <returns>Dictionary of localized messages</returns>
    [HttpGet("messages")]
    public ActionResult<object> GetMessages()
    {
        var messages = new Dictionary<string, string>
        {
            ["ValidationRequired"] = FrameworkLocalizer.GetString("ValidationRequired"),
            ["ValidationInvalidFormat"] = FrameworkLocalizer.GetString("ValidationInvalidFormat"),
            ["ValidationOutOfRange"] = FrameworkLocalizer.GetString("ValidationOutOfRange"),
            ["UnknownError"] = FrameworkLocalizer.GetString("UnknownError"),
            ["OperationSuccessful"] = FrameworkLocalizer.GetString("OperationSuccessful"),
            ["OperationFailed"] = FrameworkLocalizer.GetString("OperationFailed"),
            ["NotFound"] = FrameworkLocalizer.GetString("NotFound"),
            ["Unauthorized"] = FrameworkLocalizer.GetString("Unauthorized"),
            ["Forbidden"] = FrameworkLocalizer.GetString("Forbidden"),
            ["InvalidEntityId"] = FrameworkLocalizer.GetString("InvalidEntityId", 123),
            ["DomainEventProcessingFailed"] = FrameworkLocalizer.GetString("DomainEventProcessingFailed", "SampleEvent"),
            ["AggregateNotFound"] = FrameworkLocalizer.GetString("AggregateNotFound", 456),
            ["TransactionRolledBack"] = FrameworkLocalizer.GetString("TransactionRolledBack"),
            ["ConcurrencyConflict"] = FrameworkLocalizer.GetString("ConcurrencyConflict")
        };

        return Ok(new
        {
            Culture = System.Globalization.CultureInfo.CurrentUICulture.Name,
            Messages = messages
        });
    }
}

/// <summary>
/// User registration request with localized validation
/// </summary>
public class UserRegistrationRequest
{
    [LocalizedRequired]
    [LocalizedStringLength(50, 2)]
    public string Name { get; set; } = string.Empty;

    [LocalizedRequired]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [LocalizedRange(18, 120)]
    public int Age { get; set; }
}