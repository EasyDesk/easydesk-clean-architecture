using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.AsyncCommands;

public record CreateBestFriend(Guid PersonId, string PersonName) : IOutgoingCommand, IIncomingCommand
{
    public static string GetDestination(RoutingContext context) => context.Self;
}

public class CreateBestFriendHandler : IHandler<CreateBestFriend>
{
    private readonly IPetRepository _petRepository;

    public CreateBestFriendHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result<Nothing>> Handle(CreateBestFriend request)
    {
        var pet = Pet.Create(new Name($"{request.PersonName}'s best buddy"), request.PersonId);
        await _petRepository.SaveAndHydrate(pet);
        return Ok;
    }
}
