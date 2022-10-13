using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageSender
{
    Task Send(IMessage message, Action<MessageOptions> configure = null);

    Task SendLocal(IMessage message, Action<MessageOptions> configure = null);

    Task Defer(Duration delay, IMessage message, Action<MessageOptions> configure = null);

    Task DeferLocal(Duration delay, IMessage message, Action<MessageOptions> configure = null);

    Task Schedule(Instant instant, IMessage message, Action<MessageOptions> configure = null);

    Task ScheduleLocal(Instant instant, IMessage message, Action<MessageOptions> configure = null);
}

public class MessageOptions
{
    public Dictionary<string, string> AdditionalHeaders { get; } = new Dictionary<string, string>();

    public MessageOptions WithHeader(string key, string value)
    {
        AdditionalHeaders[key] = value;
        return this;
    }
}
