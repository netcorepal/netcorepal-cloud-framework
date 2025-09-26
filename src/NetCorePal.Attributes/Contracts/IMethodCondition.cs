using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.Contracts
{
    /// <summary>
    /// 方法过滤
    /// </summary>
    public interface IMethodCondition
    {
        bool Predicate(MethodInfo methodInfo);
    }
}
