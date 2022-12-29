namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaManager
{
    Task<Option<ISagaReference<TId, TState>>> Find<TId, TState>(TId id);

    ISagaReference<TId, TState> CreateNew<TId, TState>(TId id, TState initialState);
}
