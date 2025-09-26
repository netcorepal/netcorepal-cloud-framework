using NetCorePal.Attributes.DependencyInjection;
using System;

namespace NetCorePal.Attributes.Persistance
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RepositoryAttribute : ComponentAttribute
    {
        public string DataSource { get; set; } = string.Empty;
    }
}
