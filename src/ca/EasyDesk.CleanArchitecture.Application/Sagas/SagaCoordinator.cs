using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaCoordinator<TId, TState> : ISagaCoordinator<TId, TState>
    where TId : notnull
{
    private readonly Dictionary<TId, SagaContext<TId, TState>> _ongoingSagas = new();
    private readonly ISagaManager _sagaManager;
    private readonly SagaRegistry _sagaRegistry;

    public SagaCoordinator(ISagaManager sagaManager, SagaRegistry sagaRegistry)
    {
        _sagaManager = sagaManager;
        _sagaRegistry = sagaRegistry;
    }

    public async Task<Option<SagaContext<TId, TState>>> FindSaga(TId id)
    {
        return await _ongoingSagas.GetOption(id).Map(Some).OrElseGetAsync(() => _sagaManager
            .Find<TId, TState>(id)
            .ThenMap(x => new SagaContext<TId, TState>(x.Reference, id, x.State, isNew: false))
            .ThenIfPresent(x => RegisterNewSaga(id, x)));
    }

    public SagaContext<TId, TState> CreateNew(TId id, TState state)
    {
        var sagaReference = _sagaManager.CreateNew<TId, TState>(id);
        var context = new SagaContext<TId, TState>(sagaReference, id, state, isNew: true);
        RegisterNewSaga(id, context);
        return context;
    }

    private void RegisterNewSaga(TId id, SagaContext<TId, TState> context)
    {
        _ongoingSagas.Add(id, context);
        _sagaRegistry.RegisterNewSaga(context);
    }
}
