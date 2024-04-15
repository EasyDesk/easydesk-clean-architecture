using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record CreatePets(IEnumerable<PetInfoDto> Pets, Guid PersonId) : ICommandRequest<CreatePetsResultDto>, IAuthorize
{
    public class Validation : PimpedAbstractValidator<CreatePets>
    {
        public Validation()
        {
            RuleFor(x => x.Pets)
                .NotEmpty();

            RuleForEach(x => x.Pets)
                .SetValidator(new PetInfoDtoValidator());
        }
    }

    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPets);
}

public record CreatePetsResultDto(int Pets);

public record CreatePetsBatch : AbstractBulkOperationCommand;

public class BulkCreatePets
    : AbstractSequentialBulkOperation<
        BulkCreatePets,
        CreatePets,
        CreatePetsResultDto,
        CreatePets,
        CreatePetsBatch>
{
    private const int BatchSize = 3;
    private readonly IPetRepository _petRepository;

    public BulkCreatePets(ICommandSender commandSender, IPetRepository petRepository) : base(commandSender)
    {
        _petRepository = petRepository;
    }

    protected override async Task<CreatePets> HandleBatch(CreatePetsBatch command, CreatePets remainingWork)
    {
        foreach (var createPetCommand in remainingWork.Pets.Take(BatchSize))
        {
            var pet = Pet.Create(new Name(createPetCommand.Nickname), remainingWork.PersonId);
            await _petRepository.SaveAndHydrate(pet);
        }
        return remainingWork with
        {
            Pets = remainingWork.Pets.Skip(BatchSize),
        };
    }

    protected override CreatePetsBatch CreateCommand() => new();

    protected override Task<Result<(CreatePetsResultDto, CreatePets)>> Prepare(CreatePets command) =>
        Task.FromResult(Success((new CreatePetsResultDto(command.Pets.Count()), command)));

    protected override bool IsComplete(CreatePets remainingWork) =>
        remainingWork.Pets.IsEmpty();
}
