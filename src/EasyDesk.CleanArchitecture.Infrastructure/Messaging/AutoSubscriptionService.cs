using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class AutoSubscriptionService : IHostedService
{
    private readonly IBus _bus;
    private readonly RebusMessagingOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AutoSubscriptionService(IBus bus, RebusMessagingOptions options, IServiceScopeFactory serviceScopeFactory)
    {
        _bus = bus;
        _options = options;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(serviceScope.ServiceProvider);
        foreach (var messageType in _options.KnownMessageTypes.Where(ShouldAutoSubscribe))
        {
            await _bus.Subscribe(messageType);
        }
        await scope.CompleteAsync();
    }

    private bool ShouldAutoSubscribe(Type type)
    {
        return type.IsSubtypeOrImplementationOf(typeof(IIncomingEvent));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
