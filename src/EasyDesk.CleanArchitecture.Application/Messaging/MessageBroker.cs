using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public sealed class MessageBroker : IMessagePublisher, IMessageSender
{
    private readonly IBus _bus;

    public MessageBroker(IBus bus)
    {
        _bus = bus;
    }

    public async Task Send(IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.Send(message, headers));

    public async Task SendLocal(IMessage message, Action<MessageOptions> configure = null) =>
        await UsingConfiguredHeaders(configure, headers => _bus.SendLocal(message, headers));

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
