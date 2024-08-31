using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.MultiEnv
{
    public interface IMultiEnvServicesBuilder
    {
        IServiceCollection Services { get; }
    }


    public class MultiEnvServicesBuilder : IMultiEnvServicesBuilder
    {
        public MultiEnvServicesBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    
}
