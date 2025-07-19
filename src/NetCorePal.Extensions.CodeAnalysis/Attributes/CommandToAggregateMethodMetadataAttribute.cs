namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandToAggregateMethodMetadataAttribute : Attribute
{
    public string CommandType { get; }
    public string AggregateType { get; }
    public string MethodName { get; }
    public string[] EventTypes { get; }

    public CommandToAggregateMethodMetadataAttribute(string commandType, string aggregateType, string methodName, params string[] eventTypes)
    {
        CommandType = commandType;
        AggregateType = aggregateType;
        MethodName = methodName;
        EventTypes = eventTypes;
    }
} 