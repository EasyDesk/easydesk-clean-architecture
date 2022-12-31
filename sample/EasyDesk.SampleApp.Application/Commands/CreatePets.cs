using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

public record CreatePets(IEnumerable<CreatePet> Pets) : ICommandRequest<CreatePetsResult>;

public record CreatePetsResult(int Pets);

public record CreatePetsBatch(Guid OperationId) : AbstractBulkOperationCommand(OperationId);

public class BulkCreatePets : AbstractSequentialBulkOperation<BulkCreatePets, CreatePets, CreatePetsResult, IEnumerable<CreatePet>, CreatePetsBatch>
{
    private readonly IPetRepository _petRepository;

    public BulkCreatePets(ICommandSender commandSender, IPetRepository petRepository) : base(commandSender)
    {
        _petRepository = petRepository;
    }

    protected override async Task<Option<IEnumerable<CreatePet>>> ComputeBatch(CreatePetsBatch command, IEnumerable<CreatePet> state)
    {
        var createPetCommand = state.First();
        var pet = Pet.Create(Name.From(createPetCommand.Nickname), createPetCommand.PersonId);
        await _petRepository.SaveAndHydrate(pet);
        var remaining = state.Skip(1);
        return remaining.Any() ? Some(remaining) : None;
    }

    protected override CreatePetsBatch CreateCommand(Guid operationId) => new(operationId);

    protected override Task<(CreatePetsResult, Option<IEnumerable<CreatePet>>)> Prepare(CreatePets command) =>
        Task.FromResult((new CreatePetsResult(command.Pets.Count()), command.Pets.Any() ? Some(command.Pets) : None));
}
