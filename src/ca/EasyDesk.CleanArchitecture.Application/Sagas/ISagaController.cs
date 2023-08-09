using EasyDesk.CleanArchitecture.Application.Sagas.Builder;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaController<TId, TState>
{
    static abstract void ConfigureSaga(SagaBuilder<TId, TState> saga);
}
