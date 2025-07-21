namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandMetadataAttribute : Attribute
{
    public string CommandType { get; }

    public CommandMetadataAttribute(string commandType)
    {
        CommandType = commandType;
    }
}
