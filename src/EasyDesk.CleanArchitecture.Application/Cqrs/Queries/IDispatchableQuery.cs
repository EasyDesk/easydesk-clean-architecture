using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Queries;

public interface IDispatchableQuery<T> : IIncomingQuery, IReadOnlyOperation, IDispatchable<T>
{
}
