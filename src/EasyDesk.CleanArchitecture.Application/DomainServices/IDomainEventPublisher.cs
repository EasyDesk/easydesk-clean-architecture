using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public interface IDomainEventPublisher
{
    Task<Result<Nothing>> Publish(DomainEvent domainEvent);
}
