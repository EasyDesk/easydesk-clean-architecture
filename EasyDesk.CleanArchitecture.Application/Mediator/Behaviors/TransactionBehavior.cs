using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Responses;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Response<TResponse>>
        where TRequest : CommandBase<TResponse>
    {
        private readonly ITransactionManager _transactionManager;

        public TransactionBehavior(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Response<TResponse>> next)
        {
            await _transactionManager.Begin();
            return await next()
                .ThenIfSuccessAsync(_ => _transactionManager.Commit());
        }
    }

    public class TransactionBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
    {
        private readonly ITransactionManager _transactionManager;

        public TransactionBehaviorWrapper(ITransactionManager transactionManager)
        {
            _transactionManager = transactionManager;
        }

        protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
        {
            var behaviorType = typeof(TransactionBehavior<,>).MakeGenericType(requestType, responseType);
            return Activator.CreateInstance(behaviorType, _transactionManager) as IPipelineBehavior<TRequest, TResponse>;
        }
    }
}
