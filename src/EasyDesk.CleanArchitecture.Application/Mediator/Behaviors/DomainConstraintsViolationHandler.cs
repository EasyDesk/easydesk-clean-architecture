using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Results;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class DomainConstraintsViolationHandler<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Result<TResponse>> next)
    {
        try
        {
            return await next();
        }
        catch (DomainConstraintException ex)
        {
            var primaryError = ex.DomainErrors.First();
            var secondaryErrors = ex.DomainErrors.Skip(1);
            return Errors.Multiple(primaryError, secondaryErrors);
        }
    }
}

public class DomainConstraintsViolationHandlerWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(DomainConstraintsViolationHandler<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType) as IPipelineBehavior<TRequest, TResponse>;
    }
}
