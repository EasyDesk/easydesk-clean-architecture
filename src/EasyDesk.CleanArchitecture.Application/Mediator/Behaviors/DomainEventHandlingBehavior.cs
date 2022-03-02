using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.Tools.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class DomainEventHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : CommandBase<TResponse>
{
    private readonly DomainEventQueue _domainEventQueue;

    public DomainEventHandlingBehavior(DomainEventQueue domainEventQueue)
    {
        _domainEventQueue = domainEventQueue;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Result<TResponse>> next)
    {
        var response = await next();
        return await response.FlatTapAsync(_ => _domainEventQueue.Flush());
    }
}

public class DomainEventHandlingBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DomainEventQueue _domainEventQueue;

    public DomainEventHandlingBehaviorWrapper(DomainEventQueue domainEventQueue)
    {
        _domainEventQueue = domainEventQueue;
    }

    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(DomainEventHandlingBehavior<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType, _domainEventQueue) as IPipelineBehavior<TRequest, TResponse>;
    }
}
