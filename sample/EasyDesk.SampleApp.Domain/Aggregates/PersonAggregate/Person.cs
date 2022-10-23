using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Model;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record AlreadyMarried : DomainError;

public record NotMarriedYet : DomainError;

public record PersonCreatedEvent(Person Person) : DomainEvent;

public record PersonGotMarriedEvent(Person Person) : DomainEvent;

public record PersonDivorcedEvent(Person Person) : DomainEvent;

public class Person : AggregateRoot
{
    public Person(Guid id, Name name, bool married)
    {
        Id = id;
        Name = name;
        Married = married;
    }

    public static Person Create(Name name) => new(Guid.NewGuid(), name, married: false);

    public Guid Id { get; }

    public Name Name { get; }

    public bool Married { get; private set; }

    protected override void OnCreation()
    {
        EmitEvent(new PersonCreatedEvent(this));
    }

    public Result<Nothing> Marry()
    {
        if (Married)
        {
            return new AlreadyMarried();
        }
        Married = true;
        EmitEvent(new PersonGotMarriedEvent(this));
        return Ok;
    }

    public Result<Nothing> Divorce()
    {
        if (!Married)
        {
            return new NotMarriedYet();
        }
        Married = false;
        EmitEvent(new PersonDivorcedEvent(this));
        return Ok;
    }
}
