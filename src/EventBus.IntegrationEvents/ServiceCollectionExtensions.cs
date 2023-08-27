using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection IIntegrationEventPublishers(this IServiceCollection services)
        {


            return services;
        }
    }
}
