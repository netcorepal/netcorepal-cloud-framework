namespace NetCorePal.Extensions.Domain
{
    public interface IEntityId
    {
    }

    public interface IStronglyTypedId<out TSource> : IEntityId
    {
        TSource Id { get; }
    }

    public interface IInt64StronglyTypedId : IStronglyTypedId<Int64>
    {
    }

    public interface IInt32StronglyTypedId : IStronglyTypedId<Int32>
    {
    }

    public interface IStringStronglyTypedId : IStronglyTypedId<String>
    {
    }

    public interface IGuidStronglyTypedId : IStronglyTypedId<Guid>
    {
    }
}