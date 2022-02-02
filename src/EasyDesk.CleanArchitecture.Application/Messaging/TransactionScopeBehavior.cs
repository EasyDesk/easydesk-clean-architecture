using MediatR;
using Rebus.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public TransactionScopeBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        using (var scope = new RebusTransactionScope())
        {
            scope.TransactionContext.SetServiceProvider(_serviceProvider);

            var response = await next();

            await scope.CompleteAsync();

            return response;
        }
    }
}
