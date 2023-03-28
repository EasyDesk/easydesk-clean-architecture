using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Collections;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public class ValidationStep<T, R> : IPipelineStep<T, R>
    where R : notnull
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var validators = _serviceProvider.GetServices<IValidator<T>>().ToList();
        if (validators.IsEmpty())
        {
            return await next();
        }

        var context = new ValidationContext<T>(request);
        var errors = validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => Errors.InvalidInput(x.PropertyName, x.ErrorMessage))
            .ToList();

        return errors.Any() ? Errors.Multiple(errors.First(), errors.Skip(1)) : await next();
    }
}
