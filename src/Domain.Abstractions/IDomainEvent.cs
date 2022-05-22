using MediatR;

namespace NetCorePal.Extensions.Domain.Abstractions
{
    /// <summary>
    /// 表示一个领域事件
    /// </summary>
    public interface IDomainEvent : INotification
    {

    }
}
