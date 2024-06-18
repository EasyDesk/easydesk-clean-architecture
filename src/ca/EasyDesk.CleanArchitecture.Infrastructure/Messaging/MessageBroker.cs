using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using NodaTime;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal sealed class MessageBroker : IEventPublisher, ICommandSender
{
    private readonly IBus _bus;
    private readonly IClock _clock;
    private readonly RebusMessagingOptions _options;

    public MessageBroker(IBus bus, IClock clock, RebusMessagingOptions options)
    {
        _bus = bus;
        _clock = clock;
        _options = options;
    }

    public async Task Send<T>(T message) where T : IOutgoingCommand =>
        await _bus.Send(message, DefaultHeaders());

    public async Task Defer<T>(Duration delay, T message) where T : IOutgoingCommand
    {
        if (!_options.DeferredMessagesEnabled)
        {
            throw new InvalidOperationException("Deferred messages were not enabled.");
        }
        await _bus.Defer(delay.ToTimeSpan(), message, DefaultHeaders());
    }

    public async Task Schedule<T>(Instant instant, T message) where T : IOutgoingCommand =>
        await Defer(instant - _clock.GetCurrentInstant(), message);

    public async Task Publish<T>(T message) where T : IOutgoingEvent =>
        await _bus.Publish(message, DefaultHeaders());

    private Dictionary<string, string> DefaultHeaders()
    {
        if (_options.UseOutbox)
        {
            return new()
            {
                [TransportWithOutbox.UseOutboxHeader] = string.Empty,
            };
        }

        return [];
    }
}
