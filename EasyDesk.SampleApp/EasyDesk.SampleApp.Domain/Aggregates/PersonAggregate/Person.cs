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

    public record PersonGotMarried(Person Person) : IDomainEvent;

    public record PersonDivorced(Person Person) : IDomainEvent;

    public class Person : AggregateRoot<Person>
    {
        public Person(Guid id, Name name, bool married) : base(id)
        {
            Name = name;
            Married = married;
        }

        public Name Name { get; }

        public bool Married { get; private set; }

        public Result<Nothing> Marry()
        {
            if (Married)
            {
                return new AlreadyMarried();
            }
            Married = true;
            EmitEvent(new PersonGotMarried(this));
            return Ok;
        }

        public Result<Nothing> Divorce()
        {
            if (!Married)
            {
                return new NotMarriedYet();
            }
            Married = false;
            EmitEvent(new PersonDivorced(this));
            return Ok;
        }
    }
}
