using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore;
using NetCorePal.Extensions.AspNetCore.Validation;

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
}