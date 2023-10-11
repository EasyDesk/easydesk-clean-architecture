using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.V_1_0.AsyncCommands;

public record CreateBestFriend(Guid PersonId, string PersonName) : IOutgoingCommand, IIncomingCommand, IPropagatedCommand<CreateBestFriend, PersonCreatedEvent>
{
    public static string GetDestination(RoutingContext context) => context.Self;

    public static CreateBestFriend ToMessage(PersonCreatedEvent domainEvent) => new(
        PersonId: domainEvent.Person.Id,
        PersonName: domainEvent.Person.FirstName);
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
