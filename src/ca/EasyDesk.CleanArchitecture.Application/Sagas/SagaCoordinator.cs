using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaCoordinator<TId, TState> : ISagaCoordinator<TId, TState>
    where TId : notnull
{
    private readonly Dictionary<TId, (ISagaReference<TState> Reference, TState State)> _ongoingSagas = new();
    private readonly ISagaManager _sagaManager;
    private readonly SagaSaver _sagaSaver;

    public SagaCoordinator(ISagaManager sagaManager, SagaSaver sagaSaver)
    {
        _sagaManager = sagaManager;
        _sagaSaver = sagaSaver;
    }

    public async Task<Option<TState>> FindSaga(TId id)
    {
        return await _ongoingSagas.GetOption(id).MatchAsync(
            some: s => Task.FromResult(Some(s.State)),
            none: () => _sagaManager
                .Find<TId, TState>(id)
                .ThenIfPresent(x => _ongoingSagas.Add(id, x))
                .ThenMap(x => x.State));
    }

    public void CreateNew(TId id, TState state)
    {
        var sagaReference = _sagaManager.CreateNew<TId, TState>(id);
        _ongoingSagas.Add(id, (sagaReference, state));
    }

    public void SaveState(TId id, TState state)
    {
        var reference = _ongoingSagas[id].Reference;
        _ongoingSagas[id] = (reference, state);
        reference.UpdateState(state);
        _sagaSaver.ScheduleSave();
    }

    public void CompleteSaga(TId id)
    {
        _ongoingSagas[id].Reference.Delete();
        _ongoingSagas.Remove(id);
        _sagaSaver.ScheduleSave();
    }
}
