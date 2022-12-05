using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Sync;

public interface IQueryRequest<T> : IDispatchable<T>, IReadOnlyOperation
{
}
