using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

public record DeletePerson(Guid PersonId) : ICommandRequest<PersonSnapshot>;

public class DeletePersonHandler : MappingHandler<DeletePerson, Person, PersonSnapshot>
{
    private readonly IPersonRepository _personRepository;

    public DeletePersonHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    protected override async Task<Result<Person>> Process(DeletePerson request)
    {
        return await _personRepository
            .GetById(request.PersonId)
            .ThenOrElseError(Errors.NotFound)
            .ThenIfSuccess(_personRepository.Remove);
    }
}
