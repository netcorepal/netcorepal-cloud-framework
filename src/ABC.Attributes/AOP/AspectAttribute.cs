using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Attributes
{
    /// <summary>
    /// 切面定义
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AspectAttribute : Attribute
    {
        /// <summary>
        /// 实现了<see cref="IRegistrationCondition"/> 接口的类型
        /// </summary>
        public Type? RegistrationConditionType { get; set; }

        /// <summary>
        /// 实现了<see cref="IMethodCondition"/> 接口的类型
        /// </summary>
        public Type? MethodConditionType { get; set; }

        /// <summary>
        /// 是否跳过目标方法执行
        /// </summary>
        public bool SkipTargetExecution { get; set; } = false;

        /// <summary>
        /// 是否处理异常，如不处理异常，则异常抛出到调用线程
        /// </summary>
        public bool HandleException { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class AdviceAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class BeforeAttribute : AdviceAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AfterAttribute : AdviceAttribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ThrowingAttribute : AdviceAttribute
    {

    }
}