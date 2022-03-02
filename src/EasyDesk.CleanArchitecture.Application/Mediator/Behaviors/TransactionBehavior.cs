using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : CommandBase<TResponse>
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public TransactionBehavior(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Result<TResponse>> next)
    {
        return await _unitOfWorkProvider.RunTransactionally(() => next());
    }
}

public class TransactionBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public TransactionBehaviorWrapper(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
    {
        var behaviorType = typeof(TransactionBehavior<,>).MakeGenericType(requestType, responseType);
        return Activator.CreateInstance(behaviorType, _unitOfWorkProvider) as IPipelineBehavior<TRequest, TResponse>;
    }
}
