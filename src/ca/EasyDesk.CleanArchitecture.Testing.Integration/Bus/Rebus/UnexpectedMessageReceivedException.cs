using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public sealed class UnexpectedMessageReceivedException : Exception
{
    public UnexpectedMessageReceivedException(Type messageType, IMessage messageBody, IDictionary<string, string> headers)
        : base(
            $"""
            Message of type {messageType.Name} was received unexpectedly.

            Body:
            {messageBody}

            Headers:
            {headers.Select(kvp => $"{kvp.Key}:\n{kvp.Value}").ConcatStrings("\n\n")}
            """)
    {
        MessageType = messageType;
        MessageBody = messageBody;
        Headers = headers;
    }

    public Type MessageType { get; }

    public IMessage MessageBody { get; }

    public IDictionary<string, string> Headers { get; }
}
