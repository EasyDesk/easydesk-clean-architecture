using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaStepConfiguration<T, R, TId, TState>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> _handler;
    private readonly AsyncFunc<IServiceProvider, TId, T, Result<TState>> _sagaInitializer;

    public SagaStepConfiguration(
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

public class SagaStepConfiguration<T, TId, TState> : SagaStepConfiguration<T, Nothing, TId, TState>
{
    public SagaStepConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<Nothing>> handler,
        AsyncFunc<IServiceProvider, TId, T, Result<TState>> sagaInitializer,
        bool ignoreClosedSaga)
        : base(sagaIdProperty, handler, sagaInitializer)
    {
        IgnoreClosedSaga = ignoreClosedSaga;
    }

    public bool IgnoreClosedSaga { get; }
}
