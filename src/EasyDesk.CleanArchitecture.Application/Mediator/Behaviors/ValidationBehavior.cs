using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Response<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Response<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<Response<TResponse>> next)
    {
        var context = new ValidationContext<TRequest>(request);
        return await _validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .Select(x => Errors.InvalidProperty(x.PropertyName, x.ErrorMessage))
            .FirstOption()
            .Match(
                some: error => Task.FromResult(Failure<TResponse>(error)),
                none: async () => await next());
    }
}

public class ValidationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : RequestBase<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviorWrapper(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(ValidationBehavior<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType, _validators) as IPipelineBehavior<TRequest, TResponse>;
    }
}
