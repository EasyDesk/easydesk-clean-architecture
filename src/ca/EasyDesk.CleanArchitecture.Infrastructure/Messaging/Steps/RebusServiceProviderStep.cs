using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class RebusServiceProviderStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly ILifetimeScope _lifetimeScope;

    public RebusServiceProviderStep(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
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
        AmbientTransactionContext.Current.SetComponentContext(_lifetimeScope);
        return await next();
    }

    private async Task<Result<R>> HandleGenericContext(NextPipelineStep<R> next)
    {
        using var scope = RebusTransactionScopeUtils.CreateScopeWithComponentContext(_lifetimeScope);
        var response = await next();
        await scope.CompleteAsync();
        return response;
    }
}
