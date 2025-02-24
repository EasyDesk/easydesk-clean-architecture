namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaRegistry
{
    private readonly ISagaManager _sagaManager;
    private Action? _changesToApply;

    public SagaRegistry(ISagaManager sagaManager)
    {
        _sagaManager = sagaManager;
    }

    public void RegisterNewSaga<TId, TState>(SagaContext<TId, TState> sagaContext)
    {
        _changesToApply += sagaContext.ApplyChanges;
    }

    public async Task SaveSagaChanges()
    {
        _changesToApply?.Invoke();
        await _sagaManager.SaveAll();
    }
}
