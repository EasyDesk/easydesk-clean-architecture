using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Model;
using NodaTime;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record PersonCreatedEvent(Person Person) : DomainEvent;

public record PersonDeletedEvent(Person Person) : DomainEvent;

public class Person : AggregateRoot
{
    public Person(Guid id, Name firstName, Name lastName, LocalDate dateOfBirth, AdminId createdBy)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        CreatedBy = createdBy;
    }

    public static Person Create(Name firstName, Name lastName, LocalDate dateOfBirth, AdminId createdBy) =>
        new(Guid.NewGuid(), firstName, lastName, dateOfBirth, createdBy);

    public Guid Id { get; }

    public Name FirstName { get; }

    public Name LastName { get; }

    public LocalDate DateOfBirth { get; }

    public AdminId CreatedBy { get; }

    protected override void OnCreation()
    {
        EmitEvent(new PersonCreatedEvent(this));
    }

    protected override void OnRemoval()
    {
        EmitEvent(new PersonDeletedEvent(this));
    }
}
