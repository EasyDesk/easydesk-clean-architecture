using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Sync;

public interface ICommandRequest<T> : IDispatchable<T>, IReadWriteOperation, IRequest;
