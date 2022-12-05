using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

public record DeletePerson(Guid PersonId) : ICommandRequest<PersonSnapshot>
{
    public class Handler : MappingHandler<DeletePerson, Person, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        protected override async Task<Result<Person>> Process(DeletePerson request)
        {
            return await _personRepository
                .RequireById(request.PersonId)
                .ThenIfSuccessAsync(_personRepository.Remove);
        }
    }
}
