namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public interface ISeeder<TData> : IAsyncDisposable
{
    Task Seed(TData data);
}
