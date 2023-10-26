using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.CleanArchitecture.Testing.Unit.Commons;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

public abstract class WebServiceTestSession<T> : IAsyncDisposable
    where T : WebServiceTestsFixture<T>
{
    private readonly IList<ITestBusEndpoint> _busEndpoints = new List<ITestBusEndpoint>();
    private Option<Agent> _currentAgent;
    private readonly Lazy<HttpTestHelper> _http;
    private readonly Lazy<ITestBusEndpoint> _defaultBusEndpoint;
    private readonly Lazy<ITestBusEndpoint> _errorBusEndpoint;

    protected WebServiceTestSession(T fixture)
    {
        Fixture = fixture;
        _http = new(CreateHttpTestHelper);
        _defaultBusEndpoint = new(() => NewBusEndpoint());
        _errorBusEndpoint = new(() => NewBusEndpoint(WebService.Services.GetRequiredService<RebusMessagingOptions>().ErrorQueueName));
    }

    protected T Fixture { get; }

    protected ITestWebService WebService => Fixture.WebService;

    protected HttpTestHelper Http => _http.Value;

    protected FakeClock Clock => Fixture.Clock;

    protected ITestTenantNavigator TenantNavigator { get; } = new TestTenantNavigator();

    protected ITestBusEndpoint DefaultBusEndpoint => _defaultBusEndpoint.Value;

    protected ITestBusEndpoint ErrorBusEndpoint => _errorBusEndpoint.Value;

    protected ITestBusEndpoint NewBusEndpoint(string? inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var busEndpoint = RebusTestBusEndpoint.CreateFromServices(WebService.Services, TenantNavigator, inputQueueAddress, defaultTimeout);
        _busEndpoints.Add(busEndpoint);
        return busEndpoint;
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

    public async ValueTask DisposeAsync()
    {
        foreach (var bus in _busEndpoints)
        {
            await bus.DisposeAsync();
        }
        _busEndpoints.Clear();
        GC.SuppressFinalize(this);
    }
}
