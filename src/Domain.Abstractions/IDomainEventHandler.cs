using MediatR;

namespace NetCorePal.Extensions.Domain.Abstractions
{
    /// <summary>
    /// 表示一个领域事件处理程序
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {

    }
}
