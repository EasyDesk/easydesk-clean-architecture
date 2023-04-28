using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public sealed class SagaRequestConfiguration<T, R, TId, TState>
    where T : IDispatchable<R>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> _handler;
    private readonly AsyncFunc<IServiceProvider, TId, T, Result<TState>> _sagaInitializer;

    public SagaRequestConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler,
        AsyncFunc<IServiceProvider, TId, T, Result<TState>> sagaInitializer)
    {
        _sagaIdProperty = sagaIdProperty;
        _handler = handler;
        _sagaInitializer = sagaInitializer;
    }

    public TId GetSagaId(T request) => _sagaIdProperty(request);

    public Task<Result<R>> HandleStep(IServiceProvider provider, T request, SagaContext<TId, TState> context) =>
        _handler(provider, request, context);

    public Task<Result<TState>> InitializeSaga(IServiceProvider provider, TId sagaId, T request) =>
        _sagaInitializer(provider, sagaId, request);
}
