using EasyDesk.CleanArchitecture.Application.Cqrs.Commands;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface ICommandSender
{
    Task Send<T>(T message) where T : IOutgoingCommand, IMessage;

    Task Defer<T>(Duration delay, T message) where T : IOutgoingCommand, IMessage;

    Task Schedule<T>(Instant instant, T message) where T : IOutgoingCommand, IMessage;
}
