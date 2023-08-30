using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public sealed class MessageNotReceivedWithinTimeoutException : Exception
{
    public MessageNotReceivedWithinTimeoutException(Duration timeout, Type messageType)
        : base($"Message of type {messageType.Name} was not received within the given timeout ({timeout})")
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}
