using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Model;
using NodaTime;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record PersonCreatedEvent(Person Person) : DomainEvent;

public record PersonDeletedEvent(Person Person) : DomainEvent;

public class Person : AggregateRoot
{
    public Person(Guid id, Name firstName, Name lastName, LocalDate dateOfBirth)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }

    public static Person Create(Name firstName, Name lastName, LocalDate dateOfBirth) => new(Guid.NewGuid(), firstName, lastName, dateOfBirth);

    public Guid Id { get; }

    public Name FirstName { get; }

    public Name LastName { get; }

    public LocalDate DateOfBirth { get; }

    protected override void OnCreation()
    {
        EmitEvent(new PersonCreatedEvent(this));
    }

    protected override void OnRemoval()
    {
        EmitEvent(new PersonDeletedEvent(this));
    }
}
