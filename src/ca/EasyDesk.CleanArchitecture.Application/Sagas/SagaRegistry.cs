using EasyDesk.CleanArchitecture.Application.Data;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaRegistry
{
    private readonly ISagaManager _sagaManager;
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;
    private Action? _changesToApply;

    public SagaRegistry(ISagaManager sagaManager, IUnitOfWorkProvider unitOfWorkProvider)
    {
        _sagaManager = sagaManager;
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public void RegisterNewSaga<TId, TState>(SagaContext<TId, TState> sagaContext)
    {
        if (_changesToApply is null)
        {
            _unitOfWorkProvider.CurrentUnitOfWork.Value.BeforeCommit.Subscribe(_ => SaveSagaChanges());
        }
        _changesToApply += sagaContext.ApplyChanges;
    }

    private async Task SaveSagaChanges()
    {
        _changesToApply?.Invoke();
        await _sagaManager.SaveAll();
    }
}
