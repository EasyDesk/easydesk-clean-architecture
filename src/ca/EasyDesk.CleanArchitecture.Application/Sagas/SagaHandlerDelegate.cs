using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public delegate Task<Result<R>> SagaHandlerDelegate<T, R, TId, TState>(T request, SagaContext<TId, TState> context);
