using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Mediator.Handlers;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.Commands;

public static class CreatePerson
{
    [RequireAnyOf("People.Write")]
    public record Command(string Name) : ICommand<PersonSnapshot>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class Handler : ICommandHandler<Command, PersonSnapshot>
    {
        private readonly IPersonRepository _personRepository;

        public Handler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public Task<Result<PersonSnapshot>> Handle(Command request, CancellationToken cancellationToken)
        {
            var person = Person.Create(Name.From(request.Name));
            _personRepository.Save(person);
            return Task.FromResult(Success(new PersonSnapshot(person.Id, person.Name, person.Married)));
        }
    }
}
