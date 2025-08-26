using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Tests;

public abstract class IntegrationTest<T> : IAsyncLifetime
    where T : IntegrationTestsFixture
{
    private readonly IntegrationTestSession<T>? _session;

    protected IntegrationTestSession<T> Session => _session ?? throw new InvalidOperationException("Accessing session inside of ConfigureSession().");

    protected T Fixture { get; }

    protected IntegrationTest(T fixture)
    {
        Fixture = fixture;
        _session = new(Fixture, ConfigureSession);
    }

    protected virtual void ConfigureSession(SessionConfigurer configurer)
    {
    }

    public async ValueTask InitializeAsync()
    {
        await Fixture.BeforeTest();
        await OnInitialization();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await OnDisposal();
        await Fixture.AfterTest();
        await _session!.DisposeAsync();
    }

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
