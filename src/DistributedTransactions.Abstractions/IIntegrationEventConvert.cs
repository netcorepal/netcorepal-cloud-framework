namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventConvert<in TDomainEvent, out TIntegrationEvent>
    where TDomainEvent : notnull 
    where TIntegrationEvent : notnull
{
    public TIntegrationEvent Convert(TDomainEvent domainEvent);
}