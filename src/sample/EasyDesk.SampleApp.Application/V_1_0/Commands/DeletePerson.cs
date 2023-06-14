using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record DeletePerson(Guid PersonId) : ICommandRequest<PersonDto>, IAuthorize
{
    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPeople);
}

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
