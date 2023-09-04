using EasyDesk.CleanArchitecture.Application.Data;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaSaver
{
    private readonly ISagaManager _sagaManager;
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;
    private bool _commitHookRegistered = false;

    public SagaSaver(ISagaManager sagaManager, IUnitOfWorkProvider unitOfWorkProvider)
    {
        _sagaManager = sagaManager;
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public void ScheduleSave()
    {
        if (_commitHookRegistered)
        {
            return;
        }
        _unitOfWorkProvider.CurrentUnitOfWork.Value.BeforeCommit.Subscribe(_ => _sagaManager.SaveAll());
        _commitHookRegistered = true;
    }
}
