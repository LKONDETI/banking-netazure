using FluentValidation;
using MediatR;
using FinCore.SharedKernel.Results;

namespace FinCore.SharedKernel.PipelineBehaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // If TResponse wraps a UseCaseResult, return validation failure without throwing
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(UseCaseResult<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var method = typeof(UseCaseResult<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(UseCaseResult<object>.ValidationFailure))!;

            var errors = failures.Select(f => f.ErrorMessage);
            var result = method.Invoke(null, [errors])!;
            return (TResponse)result;
        }

        throw new ValidationException(failures);
    }
}
