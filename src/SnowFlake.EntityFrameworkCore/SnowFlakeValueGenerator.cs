using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
namespace NetCorePal.Extensions.SnowFlake.EntityFrameworkCore
{
    /// <summary>
    /// 这个类的实例由 EF Core 负责实例化,无法从容器构造，因此需要使用IdGeneratorExtension.SetupForEntityFrameworkValueGenerator来提供IdGenerator实例
    /// </summary>
    public class SnowFlakeValueGenerator : ValueGenerator
    {
        public SnowFlakeValueGenerator() { }

        public override bool GeneratesTemporaryValues => false;
        protected override object NextValue(EntityEntry entry)
        {
            return IdGenerator.NextId();
        }

        /// <summary>
        /// 
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal static SnowFlakeIdGenerator IdGenerator { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    }
}
