namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaReference<TId, TState>
{
    TId Id { get; }

    TState State { get; set; }

    Task SaveState();

    Task Delete();
}
