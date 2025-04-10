namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixture;

public interface IFixtureLifetime
{
    Task OnInitialization();

    Task BeforeTest();

    Task AfterTest();

    Task BetweenTests();

    Task OnDisposal();
}
