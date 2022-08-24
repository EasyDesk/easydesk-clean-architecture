using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mediator;
using FluentValidation;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : ICqrsRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<Result<TResponse>> next)
    {
        var context = new ValidationContext<TRequest>(request);
        var errors = _validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => Errors.InvalidInput(x.PropertyName, x.ErrorMessage))
            .ToList();

        return errors.Any() ? Errors.Multiple(errors[0], errors.Skip(1)) : await next();
    }
}
