using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

public record PersonCreatedEvent(Person Person) : DomainEvent;

public record SkipError : Error;

public class PersonCreatedHandler : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly IPersonRepository _personRepository;

    public PersonCreatedHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        ev.Person.Approved = true;
        _personRepository.Save(ev.Person);
        return Task.FromResult(Ok);
    }
}

public class PersonCreatedSkipHandler : IDomainEventHandler<PersonCreatedEvent>
{
    public PersonCreatedSkipHandler()
    {
    }

    public Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        if (ev.Person.FirstName == "skip")
        {
            return Task.FromResult<Result<Nothing>>(new SkipError());
        }
        return Task.FromResult(Ok);
    }
}
