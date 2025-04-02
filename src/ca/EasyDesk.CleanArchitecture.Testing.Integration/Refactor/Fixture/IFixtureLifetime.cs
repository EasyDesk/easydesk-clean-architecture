namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public interface IFixtureLifetime : IAsyncLifetime
{
    Task BeforeTest();

    Task AfterTest();

    Task Reset();
}
