using Columnist.Utilities.Response.Enums;
using Columnist.Utilities.Response.Errors;
using Columnist.Utilities.Response.Interfaces;
using FluentValidation;
using MediatR;

namespace Columnist.Utilities.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IValidationResponse<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Select(f => new ApiError
            {
                Classification = ErrorClassification.ValidationError,
                Field = f.PropertyName,
                Message = f.ErrorMessage
            })
            .ToList();

        if (errors.Count > 0)
            return TResponse.ValidationFailed(errors);

        return await next(cancellationToken);
    }
}
