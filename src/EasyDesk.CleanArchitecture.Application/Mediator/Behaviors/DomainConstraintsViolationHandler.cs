using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class DomainConstraintsViolationHandler<TRequest, TResponse> : IPipelineBehavior<TRequest, Response<TResponse>>
    where TRequest : RequestBase<TResponse>
{
    public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Response<TResponse>> next)
    {
        try
        {
            return await next();
        }
        catch (DomainConstraintException ex)
        {
            return Errors.FromDomain(ex.DomainErrors);
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
