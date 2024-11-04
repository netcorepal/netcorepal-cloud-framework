using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore;
using NetCorePal.Extensions.AspNetCore.Validation;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRequestCancellationToken(this IServiceCollection services)
    {
        services.AddSingleton<IRequestCancellationToken, HttpContextAccessorRequestAbortedHandler>();
        return services;
    }

    public static IServiceCollection AddKnownExceptionErrorModelInterceptor(this IServiceCollection services)
    {
        services.AddTransient<IValidatorInterceptor, KnownExceptionErrorModelInterceptor>();
        return services;
    }

    public static MediatRServiceConfiguration AddKnownExceptionValidationBehavior(
        this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(KnownExceptionValidationBehavior<,>));
        return cfg;
    }



    /// <summary>
    /// 将所有实现IQuery接口的类注册为查询类，添加到容器中
    /// </summary>
    /// <param name="services"></param>
    /// <param name="Assemblies"></param>
    /// <returns></returns>

    public static IServiceCollection AddAllQueries(this IServiceCollection services, params Assembly[] Assemblies)
    {
        foreach (var assembly in Assemblies)
        {
            //从assembly中获取所有实现IQuery接口的类
            var queryTypes = assembly.GetTypes().Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Any(i => i == typeof(IQuery)));
            foreach (var queryType in queryTypes)
            {
                //注册为自己
                services.AddTransient(queryType, queryType);
            }
        }
        return services;
    }
}