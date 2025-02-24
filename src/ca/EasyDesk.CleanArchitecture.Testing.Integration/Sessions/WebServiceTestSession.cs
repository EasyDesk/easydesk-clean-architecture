using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.CleanArchitecture.Testing.Unit.Commons;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Scopes;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Sessions;

public class AgentScope : IDisposable
{
    private readonly ScopeManager<Option<Agent>>.Scope _scope;

    public AgentScope(ScopeManager<Option<Agent>>.Scope scope)
    {
        _scope = scope;
    }

    public Option<Agent> Agent => _scope.Value;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _scope.Dispose();
    }
}

public abstract class WebServiceTestSession<T> : IAsyncDisposable
    where T : ITestFixture
{
    private readonly IList<ITestBusEndpoint> _busEndpoints = [];
    private readonly Lazy<HttpTestHelper> _http;
    private readonly Lazy<ITestBusEndpoint> _defaultBusEndpoint;
    private readonly Lazy<ITestBusEndpoint> _errorBusEndpoint;
    private readonly ScopeManager<Option<Agent>> _agentScopeManager;

    protected WebServiceTestSession(T fixture)
    {
        Fixture = fixture;
        _http = new(CreateHttpTestHelper);
        _defaultBusEndpoint = new(() => NewBusEndpoint());
        _errorBusEndpoint = new(() => NewBusEndpoint(WebService.LifetimeScope.Resolve<RebusMessagingOptions>().ErrorQueueName));
        TenantManager = new TestTenantManager(DefaultTenantInfo);
        _agentScopeManager = new(DefaultAgent);
    }

    protected T Fixture { get; }

    protected ITestWebService WebService => Fixture.WebService;

    protected HttpTestHelper Http => _http.Value;

    protected FakeClock Clock => Fixture.Clock;

    protected TestTenantManager TenantManager { get; }

    protected ITestBusEndpoint DefaultBusEndpoint => _defaultBusEndpoint.Value;

    protected ITestBusEndpoint ErrorBusEndpoint => _errorBusEndpoint.Value;

    protected virtual Option<TenantInfo> DefaultTenantInfo => None;

    protected virtual Option<Agent> DefaultAgent => None;

    protected ITestBusEndpoint NewBusEndpoint(string? inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var busEndpoint = RebusTestBusEndpoint.CreateFromServices(WebService.LifetimeScope, TenantManager, inputQueueAddress, defaultTimeout);
        _busEndpoints.Add(busEndpoint);
        return busEndpoint;
    }

    protected HttpTestHelper CreateHttpTestHelper()
    {
        var jsonSettings = WebService.LifetimeScope.Resolve<JsonOptionsConfigurator>();
        return new(WebService.HttpClient, jsonSettings, GetHttpAuthentication(), ApplyDefaultRequestConfiguration);
    }

    private void ApplyDefaultRequestConfiguration(HttpRequestBuilder req)
    {
        TenantManager.CurrentTenantInfo.FlatMap(x => x.Id).Match(
            some: req.Tenant,
            none: req.NoTenant);

        _agentScopeManager.Current.Match(
            some: req.AuthenticateAs,
            none: req.NoAuthentication);

        ConfigureRequests(req);
    }

    protected virtual void ConfigureRequests(HttpRequestBuilder req)
    {
    }

    protected AgentScope AuthenticateAs(Agent agent)
    {
        return new(_agentScopeManager.OpenScope(Some(agent)));
    }

    protected AgentScope Anonymize()
    {
        return new(_agentScopeManager.OpenScope(None));
    }

    protected virtual ITestHttpAuthentication GetHttpAuthentication() =>
        TestHttpAuthentication.CreateFromServices(WebService.LifetimeScope);

    protected async Task PollServiceUntil<TService>(AsyncFunc<TService, bool> predicate, Duration? timeout = null, Duration? interval = null) where TService : notnull
    {
        await WebService.LifetimeScope.ScopedPollUntil<ILifetimeScope>(
            async scope =>
            {
                scope
                    .ResolveOption<IContextTenantInitializer>()
                    .IfPresent(x => x.Initialize(TenantManager.CurrentTenantInfo.OrElse(TenantInfo.Public)));
                var service = scope.Resolve<TService>();
                return await predicate(service);
            },
            timeout: timeout,
            interval: interval);
    }

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
