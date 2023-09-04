using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal interface ISagaCoordinator<TId, TState>
{
    Task<Option<SagaContext<TId, TState>>> FindSaga(TId id);

    SagaContext<TId, TState> CreateNew(TId id, TState state);
}
