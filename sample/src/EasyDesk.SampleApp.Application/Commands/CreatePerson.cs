using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Application.Commands;

public static class CreatePerson
{
    public record Command(string Name) : CommandBase<PersonSnapshot>;

    public class Handler : UnitOfWorkHandler<Command, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _personRepository = personRepository;
        }

        protected override Task<Response<PersonSnapshot>> HandleRequest(Command request)
        {
            var person = Person.Create(Name.From(request.Name));
            _personRepository.Save(person);
            return Task.FromResult<Response<PersonSnapshot>>(new PersonSnapshot(person.Id, person.Name, person.Married));
        }
    }
}
