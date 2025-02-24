using Autofac;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaStepConfiguration<T, R, TId, TState>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly AsyncFunc<IComponentContext, T, SagaContext<TId, TState>, Result<R>> _handler;
    private readonly Option<AsyncFunc<IComponentContext, TId, T, Result<TState>>> _sagaInitializer;
    private readonly AsyncFunc<IComponentContext, TId, T, Result<R>> _missingSagaHandler;

    public SagaStepConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IComponentContext, T, SagaContext<TId, TState>, Result<R>> handler,
        Option<AsyncFunc<IComponentContext, TId, T, Result<TState>>> sagaInitializer,
        AsyncFunc<IComponentContext, TId, T, Result<R>> missingSagaHandler)
    {
        _sagaIdProperty = sagaIdProperty;
        _handler = handler;
        _sagaInitializer = sagaInitializer;
        _missingSagaHandler = missingSagaHandler;
    }

    public bool CanInitialize => _sagaInitializer.IsPresent;

    public TId GetSagaId(T request) => _sagaIdProperty(request);

    public Task<Result<R>> HandleStep(IComponentContext provider, T request, SagaContext<TId, TState> context) =>
        _handler(provider, request, context);

    public Task<Result<R>> HandleMissingSaga(IComponentContext provider, TId sagaId, T request) =>
        _missingSagaHandler(provider, sagaId, request);

    public Task<Result<TState>> InitializeSaga(IComponentContext provider, TId sagaId, T request) =>
        _sagaInitializer.Value(provider, sagaId, request);
}
