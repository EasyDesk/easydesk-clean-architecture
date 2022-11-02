using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Commands;

public interface IIncomingCommand<T> : ICommand, IReadWriteOperation, IDispatchable<T>
{
}
