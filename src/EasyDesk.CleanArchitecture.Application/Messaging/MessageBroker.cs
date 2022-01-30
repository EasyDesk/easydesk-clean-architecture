using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Rebus.Bus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public sealed class MessageBroker
{
    private readonly IBus _bus;
    private readonly ITimestampProvider _timestampProvider;

    public MessageBroker(IBus bus, ITimestampProvider timestampProvider)
    {
        _bus = bus;
        _timestampProvider = timestampProvider;
    }

    public async Task Send(IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Send(message, headers));

    public async Task SendLocal(IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.SendLocal(message, headers));

    public async Task Defer(Duration delay, IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Defer(delay.AsTimeSpan, message, headers));

    public async Task DeferLocal(Duration delay, IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.DeferLocal(delay.AsTimeSpan, message, headers));

    public async Task Defer(Timestamp time, IMessage message, Action<MessageOptions> configure = null) =>
        await Defer(DurationUntil(time), message, configure);

    public async Task DeferLocal(Timestamp time, IMessage message, Action<MessageOptions> configure = null) =>
        await DeferLocal(DurationUntil(time), message, configure);

    private Duration DurationUntil(Timestamp time) => Duration.FromTimeOffset(time - _timestampProvider.Now);

    public async Task Publish(IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Publish(message, headers));

    private async Task UsingConfiguredHeaders(Action<MessageOptions> configure, AsyncAction<IDictionary<string, string>> action)
    {
        var options = new MessageOptions();
        configure?.Invoke(options);
        await action(options.AdditionalHeaders);
    }
}

public class MessageOptions
{
    internal Dictionary<string, string> AdditionalHeaders { get; } = new Dictionary<string, string>();

    public MessageOptions WithHeader(string key, string value)
    {
        AdditionalHeaders[key] = value;
        return this;
    }
}
