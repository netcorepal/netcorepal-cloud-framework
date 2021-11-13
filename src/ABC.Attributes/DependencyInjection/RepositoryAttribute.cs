using System;

namespace ABC.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RepositoryAttribute : ComponentAttribute
    {
    }
}
