using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Queries;

public interface IIncomingQuery<T> : IQuery, IReadOnlyOperation, IDispatchable<T>
{
}
