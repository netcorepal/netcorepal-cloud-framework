using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.ValueGenerators;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public static class PropertyBuilderExtension
{
    public static PropertyBuilder<TEntityId> UseGuidValueGenerator<TEntityId>(
        this PropertyBuilder<TEntityId> builder)
        where TEntityId : IGuidStronglyTypedId
    {
        builder = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasValueGenerator<StrongTypeGuidValueGenerator<TEntityId>>();
    }
    
    public static PropertyBuilder<TEntityId> UseGuidVersion7ValueGenerator<TEntityId>(
        this PropertyBuilder<TEntityId> builder)
        where TEntityId : IGuidStronglyTypedId
    {
        builder = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasValueGenerator<StrongTypeGuidVersion7ValueGenerator<TEntityId>>();
    }
}