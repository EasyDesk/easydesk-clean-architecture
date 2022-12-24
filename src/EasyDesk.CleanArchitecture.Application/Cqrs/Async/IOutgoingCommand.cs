using EasyDesk.CleanArchitecture.Application.Messaging;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Async;

public interface IOutgoingCommand : ICommand, IOutgoingMessage
{
    static abstract string GetDestination(RoutingContext context);
}
