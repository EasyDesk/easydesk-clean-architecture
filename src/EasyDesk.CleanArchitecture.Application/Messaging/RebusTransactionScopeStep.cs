using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class RebusTransactionScopeStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICqrsRequest<TResult>
{
    private readonly IServiceProvider _serviceProvider;

    public RebusTransactionScopeStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
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
