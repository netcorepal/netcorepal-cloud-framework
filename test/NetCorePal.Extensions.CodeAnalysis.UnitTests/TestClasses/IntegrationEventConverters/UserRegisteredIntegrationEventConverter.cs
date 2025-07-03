using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventConverters;

/// <summary>
/// 用户注册集成事件转换器
/// </summary>
public class UserRegisteredIntegrationEventConverter : IIntegrationEventConverter<UserRegisteredDomainEvent, UserRegisteredIntegrationEvent>
{
    public UserRegisteredIntegrationEvent Convert(UserRegisteredDomainEvent domainEvent)
    {
        return new UserRegisteredIntegrationEvent
        {
            UserId = domainEvent.User.Id.Id,
            UserName = domainEvent.User.Name,
            Email = domainEvent.User.Email,
            RegisteredAt = domainEvent.RegisteredAt
        };
    }
}
