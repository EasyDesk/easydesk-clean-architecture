using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.Commands;

[RequireAnyOf(Permissions.CanEditPets)]
public record CreatePets2(IEnumerable<CreatePet> Pets) : ICommandRequest<CreatePetsResult>
{
    public class Validation : AbstractValidator<CreatePets2>
    {
        public Validation()
        {
            RuleFor(x => x.Pets)
                .NotEmpty();
        }
    }
}

public record CreatePetsBatch2 : AbstractBulkOperationCommand;

public class BulkCreatePets2
    : AbstractSequentialBulkOperation<
        BulkCreatePets2,
        CreatePets2,
        CreatePetsResult,
        IEnumerable<CreatePet>,
        CreatePetsBatch2>
{
    private const int BatchSize = 3;
    private readonly IPetRepository _petRepository;

    public BulkCreatePets2(ICommandSender commandSender, IPetRepository petRepository) : base(commandSender)
    {
        _petRepository = petRepository;
    }

    protected override async Task<IEnumerable<CreatePet>> HandleBatch(CreatePetsBatch2 command, IEnumerable<CreatePet> remainingWork)
    {
        foreach (var createPetCommand in remainingWork.Take(BatchSize))
        {
            var pet = Pet.Create(new Name(createPetCommand.Nickname), createPetCommand.PersonId);
            await _petRepository.SaveAndHydrate(pet);
        }
        return remainingWork.Skip(BatchSize);
    }

    protected override CreatePetsBatch2 CreateCommand() => new();

    protected override Task<Result<(CreatePetsResult, IEnumerable<CreatePet>)>> Prepare(CreatePets2 command) =>
        Task.FromResult(Success((new CreatePetsResult(command.Pets.Count()), command.Pets)));

    protected override bool IsComplete(IEnumerable<CreatePet> remainingWork) =>
        remainingWork.IsEmpty();
}
