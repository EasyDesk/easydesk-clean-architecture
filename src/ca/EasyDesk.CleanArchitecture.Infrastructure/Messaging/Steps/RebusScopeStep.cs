using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class RebusScopeStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly ILifetimeScope _lifetimeScope;

    public RebusScopeStep(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        if (AmbientTransactionContext.Current is not null)
        {
            return await AmbientTransactionContext.Current.UseComponentContext(_lifetimeScope, () => next());
        }

        using var scope = _lifetimeScope.CreateRebusTransactionScope();
        var response = await next();
        await scope.CompleteAsync();
        return response;
    }
}
