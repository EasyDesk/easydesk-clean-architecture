using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Commands;

public static class CreatePerson
{
    public record Command(
        string FirstName,
        string LastName,
        LocalDate DateOfBirth) : ICommand<PersonSnapshot>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }

    public class Handler : ICommandHandler<Command, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Result<PersonSnapshot>> Handle(Command request)
        {
            var person = Person.Create(Name.From(request.FirstName), Name.From(request.LastName), request.DateOfBirth);
            await _personRepository.Save(person);
            return PersonSnapshot.FromPerson(person);
        }
    }
}
