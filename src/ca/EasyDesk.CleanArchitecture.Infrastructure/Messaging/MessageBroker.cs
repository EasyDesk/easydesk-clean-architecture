﻿using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using NodaTime;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal sealed class MessageBroker : IEventPublisher, ICommandSender
{
    private readonly IBus _bus;
    private readonly IClock _clock;

    public MessageBroker(IBus bus, IClock clock)
    {
        _bus = bus;
        _clock = clock;
    }

    public async Task Send<T>(T message) where T : IOutgoingCommand =>
        await _bus.Send(message);

    public async Task Defer<T>(Duration delay, T message) where T : IOutgoingCommand =>
        await _bus.Defer(delay.ToTimeSpan(), message);

    public async Task Schedule<T>(Instant instant, T message) where T : IOutgoingCommand =>
        await Defer(instant - _clock.GetCurrentInstant(), message);

    public async Task Publish<T>(T message) where T : IOutgoingEvent =>
        await _bus.Publish(message);
}