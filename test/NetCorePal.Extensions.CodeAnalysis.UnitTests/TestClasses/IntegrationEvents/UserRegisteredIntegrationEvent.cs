namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

/// <summary>
/// 用户注册集成事件
/// </summary>
public class UserRegisteredIntegrationEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
