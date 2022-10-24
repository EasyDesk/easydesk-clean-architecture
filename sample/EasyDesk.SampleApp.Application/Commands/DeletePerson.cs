using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Commands;

public static class DeletePerson
{
    public record Command(Guid PersonId) : ICqrsRequest<PersonSnapshot>;

    public class Handler : ICqrsRequestHandler<Command, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Result<PersonSnapshot>> Handle(Command request)
        {
            return await _personRepository.RequireById(request.PersonId)
                .ThenIfSuccessAsync(_personRepository.Remove)
                .ThenMap(PersonSnapshot.FromPerson);
        }
    }
}
