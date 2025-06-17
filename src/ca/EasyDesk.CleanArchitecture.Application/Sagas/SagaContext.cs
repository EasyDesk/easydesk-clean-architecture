namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaContext<TId, TState>
{
    private readonly ISagaReference<TState> _reference;

    internal SagaContext(ISagaReference<TState> reference, TId id, TState state, bool isNew)
    {
        _reference = reference;
        Id = id;
        State = state;
        IsNew = isNew;
    }

    public TId Id { get; }

    public TState State { get; private set; }

    public bool IsComplete { get; private set; }

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

    internal void ApplyChanges()
    {
        if (IsComplete)
        {
            _reference.Delete();
        }
        else
        {
            _reference.UpdateState(State);
        }
    }
}
