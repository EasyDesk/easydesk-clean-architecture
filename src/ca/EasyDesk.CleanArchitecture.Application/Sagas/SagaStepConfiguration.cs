using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaStepConfiguration<T, R, TId, TState>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> _handler;
    private readonly Option<AsyncFunc<IServiceProvider, TId, T, Result<TState>>> _sagaInitializer;
    private readonly AsyncFunc<IServiceProvider, TId, T, Result<R>> _missingSagaHandler;

    public SagaStepConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler,
        Option<AsyncFunc<IServiceProvider, TId, T, Result<TState>>> sagaInitializer,
        AsyncFunc<IServiceProvider, TId, T, Result<R>> missingSagaHandler)
    {
        _sagaIdProperty = sagaIdProperty;
        _handler = handler;
        _sagaInitializer = sagaInitializer;
        _missingSagaHandler = missingSagaHandler;
    }

    public bool CanInitialize => _sagaInitializer.IsPresent;

    public TId GetSagaId(T request) => _sagaIdProperty(request);

    public Task<Result<R>> HandleStep(IServiceProvider provider, T request, SagaContext<TId, TState> context) =>
        _handler(provider, request, context);

    public Task<Result<R>> HandleMissingSaga(IServiceProvider provider, TId sagaId, T request) =>
        _missingSagaHandler(provider, sagaId, request);

    public Task<Result<TState>> InitializeSaga(IServiceProvider provider, TId sagaId, T request) =>
        _sagaInitializer.Value(provider, sagaId, request);
}
