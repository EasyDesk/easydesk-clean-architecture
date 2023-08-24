using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Commons;
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
    private readonly IList<RebusTestBus> _buses = new List<RebusTestBus>();
    private Option<Agent> _currentAgent;

    protected WebServiceIntegrationTest(T fixture)
    {
        Fixture = fixture;
    }

    protected T Fixture { get; }

    protected ITestWebService WebService => Fixture.WebService;

    private HttpTestHelper? _http;

    protected HttpTestHelper Http => _http!;

    protected FakeClock Clock => Fixture.Clock;

    protected ITestTenantNavigator TenantNavigator { get; } = new TestTenantNavigator();

    protected ITestBus NewBus(string? inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var bus = RebusTestBus.CreateFromServices(WebService.Services, TenantNavigator, inputQueueAddress, defaultTimeout);
        _buses.Add(bus);
        return bus;
    }

    protected HttpTestHelper CreateHttpTestHelper()
    {
        var jsonSettings = WebService.Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(WebService.HttpClient, jsonSettings, GetHttpAuthentication(), ApplyDefaultRequestConfiguration);
    }

    private void ApplyDefaultRequestConfiguration(HttpRequestBuilder req)
    {
        TenantNavigator.Tenant.Id.Match(
            some: req.Tenant,
            none: req.NoTenant);

        _currentAgent.Match(
            some: req.AuthenticateAs,
            none: req.NoAuthentication);

        ConfigureRequests(req);
    }

    protected virtual void ConfigureRequests(HttpRequestBuilder req)
    {
    }

    protected void AuthenticateAs(Agent agent)
    {
        _currentAgent = Some(agent);
    }

    protected void Anonymize()
    {
        _currentAgent = None;
    }

    protected virtual ITestHttpAuthentication GetHttpAuthentication() =>
        TestHttpAuthentication.CreateFromServices(WebService.Services);

    public async Task InitializeAsync()
    {
        _http = CreateHttpTestHelper();
        await OnInitialization();
    }

    protected virtual Task OnInitialization() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var bus in _buses)
        {
            await bus.DisposeAsync();
        }
        _buses.Clear();
        await Fixture.ResetAsync(CancellationToken.None);
        await OnDisposal();
    }

    protected virtual Task OnDisposal() => Task.CompletedTask;
}
