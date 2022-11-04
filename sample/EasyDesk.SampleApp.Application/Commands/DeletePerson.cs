using EasyDesk.CleanArchitecture.Application.Cqrs.Commands;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

public record DeletePerson(Guid PersonId) : IIncomingCommand<PersonSnapshot>
{
    public class Handler : IHandler<DeletePerson, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Result<PersonSnapshot>> Handle(DeletePerson request)
        {
            return await _personRepository.RequireById(request.PersonId)
                .ThenIfSuccessAsync(_personRepository.Remove)
                .ThenMap(PersonSnapshot.MapFrom);
        }
    }
}
