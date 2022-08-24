using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class AutoSubscriptionService : IHostedService
{
    private readonly IBus _bus;
    private readonly KnownMessageTypes _knownMessageTypes;

    public AutoSubscriptionService(IBus bus, KnownMessageTypes knownMessageTypes)
    {
        _bus = bus;
        _knownMessageTypes = knownMessageTypes;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var messageType in _knownMessageTypes.Types.Where(ShouldAutoSubscribe))
        {
            await _bus.Subscribe(messageType);
        }
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
