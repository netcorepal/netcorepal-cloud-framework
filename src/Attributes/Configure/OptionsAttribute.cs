using System;

namespace NetCorePal.Attributes.Configure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class OptionsAttribute : Attribute
    {
        public OptionsAttribute(string? group, string? name)
        {
            Group = group;
            Name = name;
        }

        public OptionsAttribute(string preConfigSection) : this(preConfigSection, null) { }

        public OptionsAttribute() : this(null, null) { }

        /// <summary>
        /// 配置项类型
        /// </summary>
        public Type? Type { get; set; }
        /// <summary>
        /// 配置项分组，默认无分组
        /// </summary>
        public string? Group { get; set; }
        /// <summary>
        /// 配置项名称
        /// </summary>
        public string? Name { get; set; }
    }
}
