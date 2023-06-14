using System;
using System.Collections.Generic;
using System.Text;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Snowflake.EntityFrameworkCore;
namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public static class SnowflakePropertyBuilderExtension
    {
        public static PropertyBuilder<TEntityId> UseSnowFlakeValueGenerator<TEntityId>(this PropertyBuilder<TEntityId> builder) where TEntityId : IEntityId
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.HasValueGenerator<SnowflakeValueGenerator<TEntityId>>();
        }
    }
}
