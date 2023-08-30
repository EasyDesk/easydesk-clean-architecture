using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

internal interface IDomainEventPublisher
{
    Task<Result<Nothing>> Publish(DomainEvent domainEvent);
}
