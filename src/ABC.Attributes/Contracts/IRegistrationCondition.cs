using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Attributes.Contracts
{

    /// <summary>
    /// 服务注册项条件接口
    /// </summary>
    public interface IRegistrationCondition
    {
        bool Predicate(ConditionContext context);
    }

    /// <summary>
    /// 条件上下文
    /// </summary>
    public class ConditionContext
    {
        public ConditionContext(IConfiguration configuration, IServiceCollection services, ServiceDescriptor service)
        {
            Configuration = configuration;
            Services = services;
            Service = service;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// 服务注册表
        /// </summary>
        public IServiceCollection Services { get; set; }

        /// <summary>
        /// 注册服务
        /// </summary>
        public ServiceDescriptor Service { get; set; }
    }
}
