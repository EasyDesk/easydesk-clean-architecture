using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal interface ISagaCoordinator<TId, TState>
{
    Task<Option<TState>> FindSaga(TId id);

    void CreateNew(TId id, TState state);

    void SaveState(TId id, TState state);

    void CompleteSaga(TId id);
}
