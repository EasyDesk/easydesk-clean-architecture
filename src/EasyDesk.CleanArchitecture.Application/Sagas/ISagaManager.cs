namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaManager
{
    Task<Option<(ISagaReference<TState> Reference, TState State)>> Find<TId, TState>(TId id) where TId : notnull;

    ISagaReference<TState> CreateNew<TId, TState>(TId id) where TId : notnull;
}
