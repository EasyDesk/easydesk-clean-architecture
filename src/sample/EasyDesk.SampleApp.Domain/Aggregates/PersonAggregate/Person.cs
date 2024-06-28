using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;
using NodaTime;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public class Person : AggregateRoot
{
    internal Person(Guid id, Name firstName, Name lastName, LocalDate dateOfBirth, AdminId createdBy, Address residence, bool approved)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        CreatedBy = createdBy;
        Residence = residence;
        Approved = approved;
    }

    public static Person Create(Name firstName, Name lastName, LocalDate dateOfBirth, AdminId createdBy, Address residence) =>
        new(Guid.NewGuid(), firstName, lastName, dateOfBirth, createdBy, residence, approved: false);

    public Guid Id { get; }

    public Name FirstName { get; set; }

    public Name LastName { get; set; }

    public LocalDate DateOfBirth { get; }

    public AdminId CreatedBy { get; }

    public Address Residence { get; set; }

    public bool Approved { get; set; }

    protected override void OnCreation()
    {
        EmitEvent(new PersonCreatedEvent(this));
    }

    protected override void OnRemoval()
    {
        EmitEvent(new PersonDeletedEvent(this));
    }
}
