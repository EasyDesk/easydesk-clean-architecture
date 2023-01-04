using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.Tools.Collections;

namespace EasyDesk.SampleApp.Application.Commands;

public record CreatePets(IEnumerable<CreatePet> Pets) : ICommandRequest<CreatePetsResult>;

public record CreatePetsResult(int Pets);

public record CreatePetsBatch(Guid OperationId) : AbstractBulkOperationCommand(OperationId);

public class BulkCreatePets : AbstractSequentialBulkOperation<BulkCreatePets, CreatePets, CreatePetsResult, IEnumerable<CreatePet>, CreatePetsBatch>
{
    private const int BatchSize = 3;
    private readonly IPetRepository _petRepository;

    public BulkCreatePets(ICommandSender commandSender, IPetRepository petRepository) : base(commandSender)
    {
        _petRepository = petRepository;
    }

    protected override async Task<IEnumerable<CreatePet>> HandleBatch(CreatePetsBatch command, IEnumerable<CreatePet> remainingWork)
    {
        foreach (var createPetCommand in remainingWork.Take(BatchSize))
        {
            var pet = Pet.Create(Name.From(createPetCommand.Nickname), createPetCommand.PersonId);
            await _petRepository.SaveAndHydrate(pet);
        }
        return remainingWork.Skip(BatchSize);
    }

    protected override CreatePetsBatch CreateCommand(Guid operationId) => new(operationId);

    protected override Task<(CreatePetsResult, IEnumerable<CreatePet>)> Prepare(CreatePets command) =>
        Task.FromResult((new CreatePetsResult(command.Pets.Count()), command.Pets));

    protected override bool IsComplete(IEnumerable<CreatePet> remainingWork) =>
        remainingWork.IsEmpty();
}
