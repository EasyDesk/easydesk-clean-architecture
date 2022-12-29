namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaContext<TState>
{
    public SagaContext(TState state)
    {
        State = state;
    }

    public TState State { get; private set; }

    public bool IsComplete { get; private set; } = false;

    public SagaContext<TState> MutateState(Func<TState, TState> update)
    {
        State = update(State);
        return this;
    }

    public SagaContext<TState> CompleteSaga()
    {
        IsComplete = true;
        return this;
    }

    internal (TState NewState, bool IsComplete) GetSagaState() => (State, IsComplete);
}
