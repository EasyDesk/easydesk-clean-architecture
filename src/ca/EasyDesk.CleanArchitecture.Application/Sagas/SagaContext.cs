namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaContext<TId, TState>
{
    public SagaContext(TId id, TState state, bool isNew)
    {
        Id = id;
        State = state;
        IsNew = isNew;
    }

    public TId Id { get; }

    public TState State { get; private set; }

    public bool IsComplete { get; private set; } = false;

    public bool IsNew { get; }

    public SagaContext<TId, TState> MutateState(Func<TState, TState> update)
    {
        State = update(State);
        return this;
    }

    public SagaContext<TId, TState> CompleteSaga()
    {
        IsComplete = true;
        return this;
    }
}
