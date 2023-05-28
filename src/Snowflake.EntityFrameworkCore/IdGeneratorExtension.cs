using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Snowflake.EntityFrameworkCore
{
    public static class IdGeneratorExtension
    {
        public static SnowflakeIdGenerator SetupForEntityFrameworkValueGenerator(this SnowflakeIdGenerator idGenerator)
        {
            SnowflakeValueGenerator.IdGenerator = idGenerator;
            return idGenerator;
        }
    }
}
