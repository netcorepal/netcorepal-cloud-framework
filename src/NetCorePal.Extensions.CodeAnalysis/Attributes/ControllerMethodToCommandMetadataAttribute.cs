namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControllerMethodToCommandMetadataAttribute : Attribute
{
    public string ControllerType { get; }
    public string MethodName { get; }
    public string[] CommandTypes { get; }

    public ControllerMethodToCommandMetadataAttribute(string controllerType, string methodName, params string[] commandTypes)
    {
        ControllerType = controllerType;
        MethodName = methodName;
        CommandTypes = commandTypes;
    }
} 