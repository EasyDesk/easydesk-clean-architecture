namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public interface IDomainEventFlusher
{
    Task<Result<Nothing>> Flush();
}
