namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventHandlerToCommandMetadataAttribute : Attribute
{
    public string HandlerType { get; }
    public string EventType { get; }
    public string[] CommandTypes { get; }

    public DomainEventHandlerToCommandMetadataAttribute(string handlerType, string eventType, params string[] commandTypes)
    {
        HandlerType = handlerType;
        EventType = eventType;
        CommandTypes = commandTypes;
    }
} 