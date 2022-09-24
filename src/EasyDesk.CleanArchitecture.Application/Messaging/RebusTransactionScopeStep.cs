using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;

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
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(_serviceProvider);
        var response = await next();
        await scope.CompleteAsync();
        return response;
    }
}
