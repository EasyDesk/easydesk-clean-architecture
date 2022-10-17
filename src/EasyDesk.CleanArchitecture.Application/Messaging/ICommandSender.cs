using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface ICommandSender
{
    Task Send(IOutgoingCommand message, Action<MessageOptions> configure = null);

    Task SendLocal(IOutgoingCommand message, Action<MessageOptions> configure = null);

    Task Defer(Duration delay, IOutgoingCommand message, Action<MessageOptions> configure = null);

    Task DeferLocal(Duration delay, IOutgoingCommand message, Action<MessageOptions> configure = null);

    Task Schedule(Instant instant, IOutgoingCommand message, Action<MessageOptions> configure = null);

    Task ScheduleLocal(Instant instant, IOutgoingCommand message, Action<MessageOptions> configure = null);
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
