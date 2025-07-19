namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventToIntegrationEventMetadataAttribute : Attribute
{
    public string DomainEventType { get; }
    public string[] IntegrationEventTypes { get; }

    public DomainEventToIntegrationEventMetadataAttribute(string domainEventType, params string[] integrationEventTypes)
    {
        DomainEventType = domainEventType;
        IntegrationEventTypes = integrationEventTypes;
    }
} 