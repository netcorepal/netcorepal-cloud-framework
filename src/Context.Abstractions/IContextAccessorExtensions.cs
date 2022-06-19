namespace NetCorePal.Context
{
    public static class IContextAccessorExtensions
    {
        public static T? GetContext<T>(this IContextAccessor accessor) where T : class
        {
            return accessor.GetContext(typeof(T)) as T;
        }
        public static IContextAccessor SetContext<T>(this IContextAccessor accessor, T context)
        {
            accessor.SetContext(typeof(T), context);
            return accessor;
        }
    }
}
