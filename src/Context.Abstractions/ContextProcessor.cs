namespace NetCorePal.Context
{
    public abstract class ContextProcessor : IContextProcessor
    {
        public virtual List<IContextSourceHandler>? SourceHandlers { get; protected set; }

        public virtual List<IContextCarrierHandler>? CarrierHandlers { get; protected set; }

        protected virtual void ExtractSource(IContextAccessor contextAccessor, IContextSource contextSource)
        {
            SourceHandlers?.ForEach(handler =>
            {
                var context = handler.Extract(contextSource);
                if (context != null) contextAccessor.SetContext(handler.ContextType, context);
            });
        }
        protected virtual void ClearContext(IContextAccessor contextAccessor)
        {
            SourceHandlers?.ForEach(handler =>
            {
                contextAccessor.SetContext(handler.ContextType, null);
            });
        }

        protected virtual void InjectCarrier(IContextAccessor contextAccessor, IContextCarrier contextCarrier)
        {
            CarrierHandlers?.ForEach(handler =>
            {
                var context = contextAccessor.GetContext(handler.ContextType);
                if (context == null) context = handler.Initial();
                handler.Inject(contextCarrier, context);
            });
        }
    }
}
