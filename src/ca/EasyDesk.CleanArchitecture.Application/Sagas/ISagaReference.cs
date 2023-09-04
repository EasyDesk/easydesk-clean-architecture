namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaReference<TState>
{
    void UpdateState(TState state);

    void Delete();
}
