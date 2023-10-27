using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Sessions;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration;

public abstract class WebServiceIntegrationTest<T> : WebServiceTestSession<T>, IAsyncLifetime
    where T : ITestFixture
{
    protected WebServiceIntegrationTest(T fixture) : base(fixture)
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
        await Fixture.AfterTest();
        await DisposeAsync();
        await Fixture.Reset();
        await OnDisposal();
    }

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
