using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Session;

public sealed class IntegrationTestSession : IAsyncDisposable
{
    private readonly Lazy<ITestBusEndpoint> _defaultBusEndpoint;
    private readonly Lazy<ITestBusEndpoint> _errorBusEndpoint;

    public IntegrationTestSession(ILifetimeScope parentScope, Action<SessionConfigurer>? configureSession = null)
    {
        LifetimeScope = configureSession is null
            ? parentScope.BeginLifetimeScope()
            : parentScope.BeginLifetimeScope(builder => configureSession(new SessionConfigurer { ContainerBuilder = builder, }));

        _defaultBusEndpoint = new(() => NewBusEndpoint());
        _errorBusEndpoint = new(() => NewBusEndpoint(Host.LifetimeScope.Resolve<RebusMessagingOptions>().ErrorQueueName));
    }

    public ILifetimeScope LifetimeScope { get; }

    public ITestHost Host => LifetimeScope.Resolve<ITestHost>();

    public FakeClock Clock => LifetimeScope.Resolve<FakeClock>();

    public HttpTestHelper Http => LifetimeScope.Resolve<HttpTestHelper>();

    public ITestBusEndpoint DefaultBusEndpoint => _defaultBusEndpoint.Value;

    public ITestBusEndpoint ErrorBusEndpoint => _errorBusEndpoint.Value;

    public TestAuthenticationManager AuthenticationManager => LifetimeScope.Resolve<TestAuthenticationManager>();

    public ITestBusEndpoint NewBusEndpoint(string? inputQueueAddress = null, Duration? defaultTimeout = null) =>
        LifetimeScope.Resolve<TestBusEndpointsManager>().NewBusEndpoint(inputQueueAddress, defaultTimeout);

    public async Task PollServiceUntil<TService>(AsyncFunc<TService, bool> predicate, Duration? timeout = null, Duration? interval = null) where TService : notnull
    {
        await Host.LifetimeScope.ScopedPollUntil<ILifetimeScope>(
            async scope =>
            {
                var service = scope.Resolve<TService>();
                return await predicate(service);
            },
            timeout: timeout,
            interval: interval);
    }

    public async ValueTask DisposeAsync()
    {
        await LifetimeScope.DisposeAsync();
    }
}
