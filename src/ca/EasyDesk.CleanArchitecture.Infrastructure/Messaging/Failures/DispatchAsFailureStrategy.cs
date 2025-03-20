using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class DispatchAsFailureStrategy : IFailureStrategy
{
    private readonly ILifetimeScope _scope;

    public DispatchAsFailureStrategy(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public async Task Handle<T>(IFailed<T> message, AsyncAction next) where T : IIncomingMessage
    {
        await using var innerScope = _scope.BeginUseCaseLifetimeScope();
        var pipelineProvider = innerScope.Resolve<IPipelineProvider>();
        var failureHandler = innerScope.ResolveOption<IFailedMessageHandler<T>>();

        await failureHandler.Match(
            some: handler => pipelineProvider
                    .GetSteps<T, Nothing>(innerScope)
                    .Run(message.Message, x => handler.HandleFailure(x))
                    .ThenIfFailureAsync(_ => next()),
            none: () => next());
    }
}
