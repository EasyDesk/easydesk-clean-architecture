namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaRequestConfiguration<T, R, TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly Func<TController, SagaHandlerDelegate<T, R, TId, TState>> _handlerFactory;
    private readonly AsyncFunc<TController, TId, T, Option<TState>> _sagaInitializer;

    public SagaRequestConfiguration(
        Func<T, TId> sagaIdProperty,
        Func<TController, SagaHandlerDelegate<T, R, TId, TState>> handlerFactory,
        AsyncFunc<TController, TId, T, Option<TState>> sagaInitializer)
    {
        _sagaIdProperty = sagaIdProperty;
        _handlerFactory = handlerFactory;
        _sagaInitializer = sagaInitializer;
    }

    public TId GetSagaId(T request) => _sagaIdProperty(request);

    public SagaHandlerDelegate<T, R, TId, TState> GetHandler(TController controller) =>
        _handlerFactory(controller);

    public Task<Option<TState>> InitializeSaga(TController controller, TId sagaId, T request) =>
        _sagaInitializer(controller, sagaId, request);
}
