using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using UUIDNext;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators
{
    public class StrongTypeGuidVersion7ValueGenerator<TEntityId> : ValueGenerator<TEntityId>
    {

        readonly System.Reflection.ConstructorInfo _constructorInfo;
        public StrongTypeGuidVersion7ValueGenerator()
        {
            var constructor = typeof(TEntityId).GetConstructor(new Type[] { typeof(Guid) }) ?? throw new Exception($"类型 {nameof(TEntityId)}必须有一个仅包含Guid类型参数的构造函数");
            _constructorInfo = constructor;
        }

        public override bool GeneratesTemporaryValues => false;

        public override TEntityId Next(EntityEntry entry)
        {
            TEntityId newObj = (TEntityId)_constructorInfo.Invoke(new object[] { Uuid.NewSequential() });
            return newObj;
        }
    }
}
