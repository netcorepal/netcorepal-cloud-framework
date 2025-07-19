namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class IntegrationEventHandlerToCommandMetadataAttribute : Attribute
{
    public string HandlerType { get; }
    public string EventType { get; }
    public string[] CommandTypes { get; }

    public IntegrationEventHandlerToCommandMetadataAttribute(string handlerType, string eventType, params string[] commandTypes)
    {
        HandlerType = handlerType;
        EventType = eventType;
        CommandTypes = commandTypes;
    }
} 