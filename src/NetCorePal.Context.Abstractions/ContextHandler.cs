
namespace NetCorePal.Context
{
    public abstract class ContextHandler : IContextSourceHandler, IContextCarrierHandler
    {
        public abstract Type ContextType { get; protected set; }

        public abstract object? Extract(IContextSource source);

        public virtual object? Initial()
        {
            return null;
        }

        public abstract void Inject(IContextCarrier carrier, object? context);
    }

    public abstract class ContextHandler<TContext> : ContextHandler
        where TContext : class
    {
        public override Type ContextType { get; protected set; } = typeof(TContext);

        public override void Inject(IContextCarrier carrier, object? context)
        {
            Inject(carrier, context as TContext);
        }
        public abstract void Inject(IContextCarrier carrier, TContext? context);
    }
}
