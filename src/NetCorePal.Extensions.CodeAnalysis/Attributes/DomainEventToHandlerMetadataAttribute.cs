namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventToHandlerMetadataAttribute : Attribute
{
    public string DomainEventType { get; }
    public string[] HandlerTypes { get; }

    public DomainEventToHandlerMetadataAttribute(string domainEventType, params string[] handlerTypes)
    {
        DomainEventType = domainEventType;
        HandlerTypes = handlerTypes;
    }
}
