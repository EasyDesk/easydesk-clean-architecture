using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

[RequireAnyOf(Permissions.CanEditPeople)]
public record DeletePerson(Guid PersonId) : ICommandRequest<PersonDto>;

public class DeletePersonHandler : MappingHandler<DeletePerson, Person, PersonDto>
{
    private readonly IPersonRepository _personRepository;

    public DeletePersonHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    protected override async Task<Result<Person>> Process(DeletePerson request)
    {
        return await _personRepository
            .FindById(request.PersonId)
            .AsOption()
            .ThenOrElseNotFound()
            .ThenIfSuccess(_personRepository.Remove);
    }
}
