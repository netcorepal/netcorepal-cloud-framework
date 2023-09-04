using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions;
namespace NetCorePal.Extensions.DistributedTransactions.CAP
{
#pragma warning disable S101 // Types should be named in PascalCase
    public class CAPIntegrationEventPublisher : IntegrationEventPublisher
#pragma warning restore S101 // Types should be named in PascalCase
    {
        readonly ICapPublisher _capPublisher;

        public CAPIntegrationEventPublisher(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) where TIntegrationEvent : notnull
        {
            return _capPublisher.PublishAsync(name: nameof(TIntegrationEvent), contentObj: integrationEvent, cancellationToken: cancellationToken);
        }
    }



}