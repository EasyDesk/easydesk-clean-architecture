using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.SampleApp.Domain.Aggregates.PetAggregate.Events;

public record PetCreatedEvent(Pet Pet) : DomainEvent;
