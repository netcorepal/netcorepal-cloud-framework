using System;

namespace NetCorePal.Context
{
    public interface IContextSourceHandler
    {
        Type ContextType { get; }
        object? Extract(IContextSource source);
    }
}
