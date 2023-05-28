using System;
using System.Collections.Generic;
using System.Text;
using NetCorePal.Extensions.Snowflake.EntityFrameworkCore;
namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public static class SnowflakePropertyBuilderExtension
    {
        public static PropertyBuilder<long> UseSnowFlakeValueGenerator(this PropertyBuilder<long> builder)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.HasValueGenerator<SnowflakeValueGenerator>();
        }
    }
}
