using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public interface IDomainEventPublisher
{
    Task<Result<Nothing>> Publish(DomainEvent domainEvent);
}
