using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.SnowFlake.EntityFrameworkCore
{
    public static class IdGeneratorExtension
    {
        public static IIdGenerator SetupForEntityFrameworkValueGenerator(this IIdGenerator idGenerator)
        {
            SnowFlakeValueGenerator.IdGenerator = idGenerator;
            return idGenerator;
        }
    }
}
