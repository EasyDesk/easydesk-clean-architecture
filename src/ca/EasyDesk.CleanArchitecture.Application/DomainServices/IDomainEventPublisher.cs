using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

internal interface IDomainEventPublisher
{
    Task<Result<Nothing>> Publish(DomainEvent domainEvent);
}
