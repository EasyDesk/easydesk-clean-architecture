using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public class RebusTransactionScopeStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IServiceProvider _serviceProvider;

    public RebusTransactionScopeStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(_serviceProvider);
        var response = await next();
        await scope.CompleteAsync();
        return response;
    }
}
