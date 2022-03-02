using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : RequestBase<TResponse>
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

public class ValidationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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
