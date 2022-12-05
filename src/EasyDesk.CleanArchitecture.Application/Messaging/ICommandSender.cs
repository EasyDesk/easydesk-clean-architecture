using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface ICommandSender
{
    Task Send<T>(T message) where T : IOutgoingCommand;

    Task Defer<T>(Duration delay, T message) where T : IOutgoingCommand;

    Task Schedule<T>(Instant instant, T message) where T : IOutgoingCommand;
}
