using Autofac;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;

public sealed class IntegrationTestSession<TFixture> : IAsyncDisposable
    where TFixture : IntegrationTestsFixture
{
    private readonly Lazy<ITestBusEndpoint> _defaultBusEndpoint;
    private readonly Lazy<ITestBusEndpoint> _errorBusEndpoint;

    public IntegrationTestSession(TFixture fixture, Action<SessionConfigurer>? configureSession = null)
    {
        Fixture = fixture;
        LifetimeScope = configureSession is null
            ? fixture.Container.BeginLifetimeScope()
            : fixture.Container.BeginLifetimeScope(builder => configureSession(new SessionConfigurer { ContainerBuilder = builder }));

        _defaultBusEndpoint = new(() => NewBusEndpoint());
        _errorBusEndpoint = new(() => NewBusEndpoint(Host.LifetimeScope.Resolve<RebusMessagingOptions>().ErrorQueueName));
    }

    public TFixture Fixture { get; }

    public ILifetimeScope LifetimeScope { get; }

    public ITestHost Host => LifetimeScope.Resolve<ITestHost>();

    public FakeClock Clock => LifetimeScope.Resolve<FakeClock>();

    public HttpTestHelper Http => LifetimeScope.Resolve<HttpTestHelper>();

    public ITestBusEndpoint DefaultBusEndpoint => _defaultBusEndpoint.Value;

    public ITestBusEndpoint ErrorBusEndpoint => _errorBusEndpoint.Value;

    public TSeed GetSeed<TSeed>() => LifetimeScope.Resolve<SeedManager<TFixture, TSeed>>().Seed;

    public TestTenantManager TenantManager => LifetimeScope.Resolve<TestTenantManager>();

    public TestAuthenticationManager AuthenticationManager => LifetimeScope.Resolve<TestAuthenticationManager>();

    public ITestBusEndpoint NewBusEndpoint(string? inputQueueAddress = null, Duration? defaultTimeout = null) =>
        LifetimeScope.Resolve<TestBusEndpointsManager>().NewBusEndpoint(inputQueueAddress, defaultTimeout);

    public async Task PollServiceUntil<TService>(AsyncFunc<TService, bool> predicate, Duration? timeout = null, Duration? interval = null) where TService : notnull
    {
        await Host.LifetimeScope.ScopedPollUntil<ILifetimeScope>(
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
        await LifetimeScope.DisposeAsync();
    }
}
