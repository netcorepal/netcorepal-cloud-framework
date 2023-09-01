using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain
{
    public interface IEntityId
    {
    }

    public interface IStronglyTypedId<TSource> : IEntityId
    {
        TSource Id { get; }
    }

    public interface IInt64StronglyTypedId : IStronglyTypedId<long>
    {
    }

    public interface IInt32StronglyTypedId : IStronglyTypedId<int>
    {
    }

    public interface IStringStronglyTypedId : IStronglyTypedId<string>
    {
    }

    public interface IGuidStronglyTypedId : IStronglyTypedId<Guid>
    {
    }
}