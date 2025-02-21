using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class RebusServiceProviderStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IServiceProvider _serviceProvider;

    public RebusServiceProviderStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsForEachHandler => false;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next) =>
        request switch
        {
            IMessage => await HandleAsyncMessageContext(next),
            _ => await HandleGenericContext(next),
        };

    private async Task<Result<R>> HandleAsyncMessageContext(NextPipelineStep<R> next)
    {
        AmbientTransactionContext.Current.SetServiceProvider(_serviceProvider);
        return await next();
    }

    private async Task<Result<R>> HandleGenericContext(NextPipelineStep<R> next)
    {
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(_serviceProvider);
        var response = await next();
        await scope.CompleteAsync();
        return response;
    }
}
