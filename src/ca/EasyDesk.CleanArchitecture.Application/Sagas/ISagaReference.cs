namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaReference<TState>
{
    Task Save(TState state);

    Task Delete();
}
