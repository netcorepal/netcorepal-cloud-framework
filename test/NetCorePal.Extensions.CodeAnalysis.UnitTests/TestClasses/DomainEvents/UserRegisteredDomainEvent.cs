using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

/// <summary>
/// 用户注册领域事件
/// </summary>
public class UserRegisteredDomainEvent : IDomainEvent
{
    public User User { get; }
    public DateTime RegisteredAt { get; }

    public UserRegisteredDomainEvent(User user, DateTime registeredAt)
    {
        User = user;
        RegisteredAt = registeredAt;
    }
}
