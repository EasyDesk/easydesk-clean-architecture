namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaController<TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    static abstract void ConfigureSaga(ISagaBuilder<TController, TId, TState> saga);

    TState GetInitialState();
}
