using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration;

public abstract class WebServiceIntegrationTest<T> : IAsyncLifetime
    where T : WebServiceTestsFixture
{
    protected WebServiceIntegrationTest(T fixture)
    {
        Fixture = fixture;
    }

    protected T Fixture { get; }

    protected ITestWebService WebService => Fixture.WebService;

    protected HttpTestHelper Http { get; private set; }

    protected FakeClock Clock => Fixture.Clock;

    protected ITestBus NewBus(string inputQueueAddress = null, Duration? defaultTimeout = null) =>
        RebusTestBus.CreateFromServices(WebService.Services, inputQueueAddress, defaultTimeout);

    public HttpTestHelper CreateHttpTestHelper()
    {
        var jsonSettings = WebService.Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(WebService.HttpClient, jsonSettings, GetHttpAuthentication(), ConfigureRequests);
    }

    protected virtual void ConfigureRequests(HttpRequestBuilder req)
    {
    }

    protected virtual ITestHttpAuthentication GetHttpAuthentication() =>
        TestHttpAuthentication.CreateFromServices(WebService.Services);

    public async Task InitializeAsync()
    {
        Http = CreateHttpTestHelper();
        await Fixture.ResetAsync(new CancellationTokenSource().Token);
        await OnInitialization();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    public async Task DisposeAsync() => await OnDisposal();

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
