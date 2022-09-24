using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class AutoSubscriptionService : IHostedService
{
    private readonly IBus _bus;
    private readonly KnownMessageTypes _knownMessageTypes;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AutoSubscriptionService(IBus bus, KnownMessageTypes knownMessageTypes, IServiceScopeFactory serviceScopeFactory)
    {
        _bus = bus;
        _knownMessageTypes = knownMessageTypes;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(serviceScope.ServiceProvider);
        foreach (var messageType in _knownMessageTypes.Types.Where(ShouldAutoSubscribe))
        {
            await _bus.Subscribe(messageType);
        }
        await scope.CompleteAsync();
    }

    private bool ShouldAutoSubscribe(Type type)
    {
        var autoSubscribeAttribute = type
            .GetTypeInfo()
            .GetCustomAttribute<RebusAutoSubscribeAttribute>();
        return autoSubscribeAttribute is not null;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
