namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AggregateMethodEventMetadataAttribute : Attribute
{
    public string AggregateType { get; }
    public string MethodName { get; }
    public string[] EventTypes { get; }

    public AggregateMethodEventMetadataAttribute(string aggregateType, string methodName, params string[] eventTypes)
    {
        AggregateType = aggregateType;
        MethodName = methodName;
        EventTypes = eventTypes;
    }
} 