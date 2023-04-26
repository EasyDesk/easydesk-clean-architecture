using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class RebusServiceProviderStep<T, R> : IPipelineStep<T, R>
    where R : notnull
    where T : IReadWriteOperation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IContextProvider _contextProvider;

    public RebusServiceProviderStep(IServiceProvider serviceProvider, IContextProvider contextProvider)
    {
        _serviceProvider = serviceProvider;
        _contextProvider = contextProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next) =>
        _contextProvider.CurrentContext switch
        {
            ContextInfo.AsyncMessage => await HandleAsyncMessageContext(next),
            _ => await HandleGenericContext(next)
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
