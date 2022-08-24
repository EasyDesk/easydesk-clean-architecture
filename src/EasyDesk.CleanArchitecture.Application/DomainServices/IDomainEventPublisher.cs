using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public interface IDomainEventPublisher
{
    Task<Result<Nothing>> Publish(DomainEvent domainEvent);
}
