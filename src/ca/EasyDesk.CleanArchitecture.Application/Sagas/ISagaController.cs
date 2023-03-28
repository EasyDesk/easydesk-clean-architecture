namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaController<TId, TState>
    where TState : notnull
{
    static abstract void ConfigureSaga(SagaBuilder<TId, TState> saga);
}
