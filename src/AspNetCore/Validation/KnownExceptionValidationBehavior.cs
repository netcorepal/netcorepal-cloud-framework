using FluentValidation;
using MediatR;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.Validation;

public sealed class KnownExceptionValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public KnownExceptionValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var errorsDictionary = (await Task.WhenAll(
            _validators.Select(x => x.ValidateAsync(context, cancellationToken)))
            )
            .SelectMany(x => x.Errors)
            .Where(x => x != null).ToList();
        if (errorsDictionary.Any())
        {
            throw new KnownException(message: errorsDictionary[0].ErrorMessage, errorCode: 400,
                errorData: errorsDictionary.Select(p =>
                    new { errorCode = p.ErrorCode, errorMessage = p.ErrorMessage, propertyName = p.PropertyName }
                ).ToArray<object>());
        }

        return await next();
    }
}