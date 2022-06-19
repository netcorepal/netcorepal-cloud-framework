using System;

namespace NetCorePal.Context
{
    public interface IContextAccessor
    {
        object? GetContext(Type type);
        void SetContext(Type type, object? context);
    }
}
