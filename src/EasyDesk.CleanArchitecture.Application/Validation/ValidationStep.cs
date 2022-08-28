using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public class ValidationStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICqrsRequest<TResult>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationStep(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
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
