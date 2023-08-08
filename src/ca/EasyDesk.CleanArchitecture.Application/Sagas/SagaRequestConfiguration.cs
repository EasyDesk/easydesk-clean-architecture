using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaRequestConfiguration<T, R, TId, TState> : SagaStepConfiguration<T, R, TId, TState>
    where T : IDispatchable<R>
{
    public SagaRequestConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler,
        AsyncFunc<IServiceProvider, TId, T, Result<TState>> sagaInitializer)
        : base(sagaIdProperty, handler, sagaInitializer)
    {
    }
}
