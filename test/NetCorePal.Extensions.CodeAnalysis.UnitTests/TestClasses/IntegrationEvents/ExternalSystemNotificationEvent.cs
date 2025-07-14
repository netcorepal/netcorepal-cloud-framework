namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

/// <summary>
/// 外部系统通知事件 - 没有对应的领域事件转换器
/// 这个事件直接由外部系统发布，不是从领域事件转换而来
/// </summary>
public class ExternalSystemNotificationEvent
{
    public Guid NotificationId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string EventType { get; set; } = string.Empty;
}
