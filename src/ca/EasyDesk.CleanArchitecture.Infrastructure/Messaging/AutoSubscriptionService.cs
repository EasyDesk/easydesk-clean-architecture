using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.Commons.Reflection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class AutoSubscriptionService : IHostedService
{
    private readonly IBus _bus;
    private readonly RebusMessagingOptions _options;
    private readonly ILifetimeScope _lifetimeScope;

    public AutoSubscriptionService(IBus bus, RebusMessagingOptions options, ILifetimeScope lifetimeScope)
    {
        _bus = bus;
        _options = options;
        _lifetimeScope = lifetimeScope;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var childScope = _lifetimeScope.BeginUseCaseLifetimeScope();
        using var rebusScope = childScope.CreateRebusTransactionScope();
        foreach (var messageType in _options.KnownMessageTypes.Where(ShouldAutoSubscribe))
        {
            await _bus.Subscribe(messageType);
        }
        await rebusScope.CompleteAsync();
    }

    private bool ShouldAutoSubscribe(Type type)
    {
        return type.IsSubtypeOrImplementationOf(typeof(IIncomingEvent));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
