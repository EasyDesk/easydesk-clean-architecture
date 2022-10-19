using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using NodaTime;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public sealed class MessageBroker : IEventPublisher, ICommandSender
{
    private readonly IBus _bus;
    private readonly IClock _clock;

    public MessageBroker(IBus bus, IClock clock)
    {
        _bus = bus;
        _clock = clock;
    }

    public async Task Send(IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Send(message, headers));

    public async Task SendLocal(IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.SendLocal(message, headers));

    public async Task Defer(Duration delay, IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Defer(delay.ToTimeSpan(), message, headers));

    public async Task DeferLocal(Duration delay, IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.DeferLocal(delay.ToTimeSpan(), message, headers));

    public async Task Schedule(Instant instant, IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await Defer(instant - _clock.GetCurrentInstant(), message, configure);

    public async Task ScheduleLocal(Instant instant, IOutgoingCommand message, Action<MessageOptions> configure = null) =>
        await DeferLocal(instant - _clock.GetCurrentInstant(), message, configure);

    public async Task Publish(IOutgoingEvent message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Publish(message, headers));

    private async Task UsingConfiguredHeaders(Action<MessageOptions> configure, AsyncAction<IDictionary<string, string>> action)
    {
        var options = new MessageOptions();
        configure?.Invoke(options);
        await action(options.AdditionalHeaders);
    }
}
