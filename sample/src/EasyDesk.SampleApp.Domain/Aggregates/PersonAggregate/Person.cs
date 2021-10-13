using EasyDesk.CleanArchitecture.Domain;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools;
using System;
using static EasyDesk.CleanArchitecture.Domain.Metamodel.Results.ResultImports;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate
{
    public record AlreadyMarried : DomainError;

    public record NotMarriedYet : DomainError;

    public record PersonCreatedEvent(Person Person) : IDomainEvent;

    public record PersonGotMarriedEvent(Person Person) : IDomainEvent;

    public record PersonDivorcedEvent(Person Person) : IDomainEvent;

    public class Person : AggregateRoot
    {
        public Person(Guid id, Name name, bool married)
        {
            Id = id;
            Name = name;
            Married = married;
        }

        public static Person Create(Name name)
        {
            var person = new Person(Guid.NewGuid(), name, married: false);
            person.EmitEvent(new PersonCreatedEvent(person));
            return person;
        }

        public Guid Id { get; }

        public Name Name { get; }

        public bool Married { get; private set; }

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
}
