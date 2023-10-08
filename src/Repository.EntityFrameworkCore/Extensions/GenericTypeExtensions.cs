namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Extensions
{
    public static class GenericTypeExtensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var typeName = string.Empty;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static string GetGenericTypeName(this object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return instance.GetType().GetGenericTypeName();
        }
    }
}
