using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class DispatchAsFailureStrategy : IFailureStrategy
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DispatchAsFailureStrategy(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Handle<T>(IFailed<T> message, AsyncAction next) where T : IIncomingMessage
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var pipelineProvider = scope.ServiceProvider.GetRequiredService<IPipelineProvider>();
        var failureHandler = scope.ServiceProvider.GetServiceAsOption<IFailedMessageHandler<T>>();

        using var rebusScope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(scope.ServiceProvider);

        await failureHandler.Match(
            some: handler => pipelineProvider
                .GetSteps<T, Nothing>(scope.ServiceProvider)
                .Run(message.Message, x => handler.HandleFailure(x))
                .ThenIfFailureAsync(_ => next()),
            none: () => next());

        await rebusScope.CompleteAsync();
    }
}
