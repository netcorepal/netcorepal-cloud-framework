using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventConverter<in TDomainEvent, out TIntegrationEvent>
    where TDomainEvent : IDomainEvent 
    where TIntegrationEvent : notnull
{
    public TIntegrationEvent Convert(TDomainEvent domainEvent);
}