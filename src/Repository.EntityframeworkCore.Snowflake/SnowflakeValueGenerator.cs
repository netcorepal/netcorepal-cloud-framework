using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Snowflake;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Snowflake
{
    /// <summary>
    /// 这个类的实例由 EF Core 负责实例化,无法从容器构造，因此需要使用IdGeneratorExtension.SetupForEntityFrameworkValueGenerator来提供IdGenerator实例
    /// </summary>
    public class SnowflakeValueGenerator<TEntityId> : ValueGenerator<TEntityId> where TEntityId : IEntityId
    {
        readonly System.Reflection.ConstructorInfo _constructorInfo;

        public SnowflakeValueGenerator()
        {
            var constructor = typeof(TEntityId).GetConstructor(new Type[] { typeof(long) });
            if (constructor == null)
            {
                throw new Exception($"类型 {nameof(TEntityId)}必须有一个仅包含long类型参数的构造函数");
            }

            _constructorInfo = constructor;

        }

        public override bool GeneratesTemporaryValues => false;


        public override TEntityId Next(EntityEntry entry)
        {
            return (TEntityId)_constructorInfo.Invoke(new object[] { SnowflakeIdGenerator.GetNextValue() });
        }
    }
}
