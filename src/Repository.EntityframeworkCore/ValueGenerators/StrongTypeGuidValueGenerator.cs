using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NetCorePal.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.ValueGenerators
{
    public class StrongTypeGuidValueGenerator<TEntityId> : ValueGenerator<TEntityId>
    {

        System.Reflection.ConstructorInfo _constructorInfo;
        public StrongTypeGuidValueGenerator()
        {
            var constructor = typeof(TEntityId).GetConstructor(new Type[] { typeof(Guid) });
            if (constructor == null)
            {
                throw new Exception($"类型 {nameof(TEntityId)}必须有一个仅包含Guid类型参数的构造函数");
            }
            _constructorInfo = constructor;
        }

        public override bool GeneratesTemporaryValues => false;

        public override TEntityId Next(EntityEntry entry)
        {
            TEntityId newObj = (TEntityId)_constructorInfo.Invoke(new object[] { Guid.NewGuid() });
            return newObj;
        }
    }
}
