using System;
using System.Collections.Generic;
using System.Threading;
using NetCorePal.Context;

namespace NetCorePal.Context
{
    public class ContextAccessor : IContextAccessor
    {
        public static ContextAccessor Instance { get; } = new ContextAccessor();

        private readonly AsyncLocal<Dictionary<Type, object?>> asyncLocal = new();
        public object? GetContext(Type type)
        {
            return asyncLocal.Value?.TryGetValue(type, out object? context) == true ? context : null;
        }

        public void SetContext(Type type, object? context)
        {
            asyncLocal.Value ??= new Dictionary<Type, object?>();
            asyncLocal.Value[type] = context;
        }
    }
}
