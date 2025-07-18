namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandSenderToCommandMetadataAttribute : Attribute
{
    public string SenderType { get; }
    public string MethodName { get; }
    public string[] CommandTypes { get; }

    public CommandSenderToCommandMetadataAttribute(string senderType, string methodName, params string[] commandTypes)
    {
        SenderType = senderType;
        MethodName = methodName;
        CommandTypes = commandTypes;
    }
} 