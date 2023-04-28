using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaStepHandler<T, R, TId, TState>
    where T : IDispatchable<R>
{
    Task<Result<R>> Handle(T request, SagaContext<TId, TState> context);
}

public interface ISagaStepHandler<T, TId, TState> : ISagaStepHandler<T, Nothing, TId, TState>
    where T : IDispatchable<Nothing>
{
}
