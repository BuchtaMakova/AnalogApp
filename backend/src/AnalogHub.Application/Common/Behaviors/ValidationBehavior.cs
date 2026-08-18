using FluentValidation;
using MediatR;

namespace AnalogHub.Application.Common.Behaviors;

/// <summary>Runs all registered FluentValidation validators for a request before its handler executes.</summary>
/// <remarks>
/// Constrained only to <c>notnull</c> (matching <see cref="IPipelineBehavior{TRequest,TResponse}"/>
/// itself), not <c>TRequest : IRequest&lt;TResponse&gt;</c> — the plain, non-generic <see cref="IRequest"/>
/// void commands use (e.g. <c>UpdatePhotoRatingCommand : IRequest</c>) does NOT implement
/// <c>IRequest&lt;Unit&gt;</c> in MediatR 12, so that stricter constraint made the DI container
/// unable to close this generic for TResponse=Unit at all: <c>GetServices&lt;IPipelineBehavior&lt;TRequest,Unit&gt;&gt;()</c>
/// silently returned empty, and every void command's validator was skipped — invalid input (e.g. a
/// rating of 9) sailed through to the database's check constraint as a raw 500 instead of a 400.
/// </remarks>
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
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
