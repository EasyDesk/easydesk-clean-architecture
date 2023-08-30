using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Commons.Results;
using System.Diagnostics;

namespace EasyDesk.SampleApp.Application.V_1_1.AsyncCommands;

public record CreateBestFriend(Guid PersonId, string PersonName) : IOutgoingCommand, IIncomingCommand
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public class CreateBestFriendHandler : IHandler<CreateBestFriend>
{
    public Task<Result<Nothing>> Handle(CreateBestFriend request)
    {
        return Task.FromException<Result<Nothing>>(new UnreachableException("Should never be received"));
    }
}
