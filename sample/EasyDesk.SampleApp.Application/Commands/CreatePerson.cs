using EasyDesk.CleanArchitecture.Application.Cqrs.Commands;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Commands;

public record CreatePerson(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth) : IIncomingCommand<PersonSnapshot>
{
    public class Validator : AbstractValidator<CreatePerson>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }

    public class Handler : IHandler<CreatePerson, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Result<PersonSnapshot>> Handle(CreatePerson command)
        {
            var person = Person.Create(Name.From(command.FirstName), Name.From(command.LastName), command.DateOfBirth);
            await _personRepository.Save(person);
            return PersonSnapshot.FromPerson(person);
        }
    }
}
