using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

public record PersonCreatedEvent(Person Person) : DomainEvent;

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
