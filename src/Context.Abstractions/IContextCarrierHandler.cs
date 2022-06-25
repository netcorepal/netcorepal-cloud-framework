using System;

namespace NetCorePal.Context
{
    public interface IContextCarrierHandler
    {
        Type ContextType { get; }
        void Inject(IContextCarrier carrier, object? context);
        object? Initial();
    }
}
