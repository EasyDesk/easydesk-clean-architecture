namespace EasyDesk.CleanArchitecture.Application.Sagas;

public delegate Task<Result<R>> SagaHandlerDelegate<T, R, TState>(T request, SagaContext<TState> context);
