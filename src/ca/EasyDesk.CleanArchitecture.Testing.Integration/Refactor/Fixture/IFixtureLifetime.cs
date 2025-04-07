namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public interface IFixtureLifetime
{
    Task OnInitialization();

    Task BeforeTest();

    Task AfterTest();

    Task BetweenTests();

    Task OnDisposal();
}
