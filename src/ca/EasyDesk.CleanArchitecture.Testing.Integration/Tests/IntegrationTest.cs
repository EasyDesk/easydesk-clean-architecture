using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Tests;

public abstract class IntegrationTest<T> : IAsyncLifetime
    where T : IntegrationTestsFixture
{
    protected IntegrationTestSession<T> Session { get; }

    protected T Fixture => Session.Fixture;

    protected IntegrationTest(T fixture)
    {
        Session = new(fixture, ConfigureSession);
    }

    protected virtual void ConfigureSession(SessionConfigurer configurer)
    {
    }

    public async Task InitializeAsync()
    {
        await Fixture.BeforeTest();
        await OnInitialization();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await OnDisposal();
        await Fixture.AfterTest();
        await Session.DisposeAsync();
    }

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
