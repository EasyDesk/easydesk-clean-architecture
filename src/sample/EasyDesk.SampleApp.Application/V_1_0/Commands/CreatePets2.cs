using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[RequireAnyOf(Permissions.CanEditPets)]
public record CreatePets2(IEnumerable<PetInfoDto> Pets, Guid PersonId) : ICommandRequest<CreatePetsResultDto>
{
    public class Validation : PimpedAbstractValidator<CreatePets2>
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
        CreatePetsResultDto,
        CreatePets2,
        CreatePetsBatch2>
{
    private const int BatchSize = 3;
    private readonly IPetRepository _petRepository;

    public BulkCreatePets2(ICommandSender commandSender, IPetRepository petRepository) : base(commandSender)
    {
        _petRepository = petRepository;
    }

    protected override async Task<CreatePets2> HandleBatch(CreatePetsBatch2 command, CreatePets2 remainingWork)
    {
        foreach (var createPetCommand in remainingWork.Pets.Take(BatchSize))
        {
            var pet = Pet.Create(new Name(createPetCommand.Nickname), remainingWork.PersonId);
            await _petRepository.SaveAndHydrate(pet);
        }
        return remainingWork with
        {
            Pets = remainingWork.Pets.Skip(BatchSize)
        };
    }

    protected override CreatePetsBatch2 CreateCommand() => new();

    protected override Task<Result<(CreatePetsResultDto, CreatePets2)>> Prepare(CreatePets2 command) =>
        Task.FromResult(Success((new CreatePetsResultDto(command.Pets.Count()), command)));

    protected override bool IsComplete(CreatePets2 remainingWork) =>
        remainingWork.Pets.IsEmpty();
}
