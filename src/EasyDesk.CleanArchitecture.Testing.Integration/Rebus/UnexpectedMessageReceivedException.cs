using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Rebus;

public class UnexpectedMessageReceivedException : Exception
{
    public UnexpectedMessageReceivedException(Duration timeout, Type messageType)
        : base($"Message of type {messageType.Name} was received unexpectedly within the given timeout ({timeout})")
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}
