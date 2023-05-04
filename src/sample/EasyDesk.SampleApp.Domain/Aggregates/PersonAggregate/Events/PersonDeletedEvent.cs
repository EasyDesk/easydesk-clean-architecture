using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

public record PersonDeletedEvent(Person Person) : DomainEvent;
