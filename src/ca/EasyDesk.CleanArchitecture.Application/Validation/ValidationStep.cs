using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public sealed class ValidationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsForEachHandler => false;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var validators = _serviceProvider.GetServices<IValidator<T>>().ToList();
        if (validators.IsEmpty())
        {
            return await next();
        }

        return await ValidationUtils.Validate(request, validators)
            .FlatMapAsync(_ => next());
    }
}
