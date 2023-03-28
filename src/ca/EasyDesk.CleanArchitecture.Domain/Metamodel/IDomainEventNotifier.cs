namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public interface IDomainEventNotifier
{
    void Notify(DomainEvent domainEvent);
}
