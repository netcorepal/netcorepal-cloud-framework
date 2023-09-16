using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Snowflake;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public static class SnowflakePropertyBuilderExtension
    {
        public static PropertyBuilder<TEntityId> UseSnowFlakeValueGenerator<TEntityId>(
            this PropertyBuilder<TEntityId> builder) where TEntityId : IEntityId
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.HasValueGenerator<SnowflakeValueGenerator<TEntityId>>();
        }
    }
}