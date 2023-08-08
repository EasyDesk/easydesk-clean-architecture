using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaStepRequestHandler<T, R, TId, TState>
    where T : IDispatchable<R>
{
    Task<Result<R>> Handle(T request, SagaContext<TId, TState> context);
}

public interface ISagaStepRequestHandler<T, TId, TState> : ISagaStepRequestHandler<T, Nothing, TId, TState>
    where T : IDispatchable<Nothing>
{
}
