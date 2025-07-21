namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandToAggregateMethodMetadataAttribute : Attribute
{
    public string CommandType { get; }
    public string AggregateType { get; }
    public string MethodName { get; }

    public CommandToAggregateMethodMetadataAttribute(string commandType, string aggregateType, string methodName)
    {
        CommandType = commandType;
        AggregateType = aggregateType;
        MethodName = methodName;
    }
} 