namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaRequestConfiguration<T, R, TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly Func<T, TId> _sagaIdProperty;
    private readonly Func<TController, SagaHandlerDelegate<T, R, TState>> _handlerFactory;

    public SagaRequestConfiguration(
        Func<T, TId> sagaIdProperty,
        Func<TController, SagaHandlerDelegate<T, R, TState>> handlerFactory,
        bool canStartSaga)
    {
        _sagaIdProperty = sagaIdProperty;
        _handlerFactory = handlerFactory;
        CanStartSaga = canStartSaga;
    }

    public bool CanStartSaga { get; }

    public TId GetSagaId(T request) => _sagaIdProperty(request);

    public SagaHandlerDelegate<T, R, TState> GetHandler(TController controller) => _handlerFactory(controller);
}
