namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class IntegrationEventToHandlerMetadataAttribute : Attribute
{
    public string IntegrationEventType { get; }
    public string[] HandlerTypes { get; }

    public IntegrationEventToHandlerMetadataAttribute(string integrationEventType, params string[] handlerTypes)
    {
        IntegrationEventType = integrationEventType;
        HandlerTypes = handlerTypes;
    }
}
