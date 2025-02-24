using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public sealed class ValidationStep<T, R> : IPipelineStep<T, R>
{
    private readonly IComponentContext _componentContext;

    public ValidationStep(IComponentContext componentContext)
    {
        _componentContext = componentContext;
    }

    public bool IsForEachHandler => false;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var validators = _componentContext.Resolve<IEnumerable<IValidator<T>>>().ToList();
        if (validators.IsEmpty())
        {
            return await next();
        }

        return await ValidationUtils.Validate(request, validators)
            .FlatMapAsync(_ => next());
    }
}
