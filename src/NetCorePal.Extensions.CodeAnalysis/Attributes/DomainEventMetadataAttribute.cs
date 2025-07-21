namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventMetadataAttribute : Attribute
{
    public string DomainEventType { get; }

    public DomainEventMetadataAttribute(string domainEventType)
    {
        DomainEventType = domainEventType;
    }
}
