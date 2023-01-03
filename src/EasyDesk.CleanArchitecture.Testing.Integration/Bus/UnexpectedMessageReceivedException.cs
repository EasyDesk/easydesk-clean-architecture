namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus;

public class UnexpectedMessageReceivedException : Exception
{
    public UnexpectedMessageReceivedException(Type messageType)
        : base($"Message of type {messageType.Name} was received unexpectedly")
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}
