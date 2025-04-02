using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Observables;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public abstract class IntegrationTestsFixture : IFixtureLifetime, ITimedFixture
{
    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers = new();
    private readonly SimpleAsyncEvent<Nothing> _onInitialization = new();
    private readonly SimpleAsyncEvent<Nothing> _beforeEachTest = new();
    private readonly SimpleAsyncEvent<Nothing> _afterEachTest = new();
    private readonly SimpleAsyncEvent<Nothing> _onReset = new();
    private readonly SimpleAsyncEvent<Nothing> _onDisposal = new();
    private readonly Instant _createdInstant = SystemClock.Instance.GetCurrentInstant();

    protected IntegrationTestsFixture(Type entryPointMarker)
    {
        Clock = new(InitialInstant);
        SetupFixture();
    }

    private ITestWebService? _webService;

    public ITestWebService WebService => _webService!;

    public FakeClock Clock { get; }

    protected virtual Instant InitialInstant => _createdInstant;

    private void SetupFixture()
    {
        var builder = new WebServiceTestsFixtureBuilder<TSelf>(
            webServiceBuilder: _webServiceBuilder,
            containers: _containers,
            onInitialization: _onInitialization,
            beforeEachTest: _beforeEachTest,
            afterEachTest: _afterEachTest,
            onReset: _onReset,
            onDisposal: _onDisposal);

        ConfigureFixture(builder);
    }

    protected abstract void ConfigureFixture(WebServiceTestsFixtureBuilder<TSelf> builder);

    private ITestWebService StartService() => _webServiceBuilder.Build();

    public async Task InitializeAsync()
    {
        await StartFixture();
        await InitializeInternal();
    }

    internal virtual async Task InitializeInternal() => await _onInitialization.Emit(GetSelf());

    private async Task StartFixture()
    {
        await _containers.StartAll();
        _webService = StartService();
    }

    private TSelf GetSelf() => (TSelf)this;

    public async Task BeforeTest() => await BeforeTestInternal();

    internal virtual async Task BeforeTestInternal() => await _beforeEachTest.Emit(GetSelf());

    public async Task AfterTest() => await AfterTestInternal();

    internal virtual async Task AfterTestInternal() => await _afterEachTest.Emit(GetSelf());

    public async Task Reset()
    {
        var hostedServicesToStop = WebService
            .LifetimeScope
            .Resolve<IEnumerable<IHostedService>>()
            .SelectMany(h => h is IPausableHostedService p ? Some(p) : None)
            .ToList();

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Pause(CancellationToken.None);
        }

        await ResetInternal();

        Clock.Reset(InitialInstant);

        foreach (var hostedService in hostedServicesToStop)
        {
            await hostedService.Resume(CancellationToken.None);
        }
    }

    internal virtual async Task ResetInternal() => await _onReset.Emit(GetSelf());

    public async Task DisposeAsync()
    {
        await DisposeInternal();
        await WebService.DisposeAsync();
        await _containers.DisposeAsync();
    }

    internal virtual async Task DisposeInternal() => await _onDisposal.Emit(GetSelf());

    public abstract class WithSeeding<TSeed> : IntegrationTestsFixture<TSelf>
    {
        private Option<TSeed> _currentSeed = None;

        private WithSeeding(Type entryPointMarker) : base(entryPointMarker)
        {
        }

        public TSeed Seed => _currentSeed.OrElseThrow(() => new InvalidOperationException("Accessing seed outside of its lifetime."));

        private async Task ApplySeed()
        {
            await using var seeder = CreateSeeder(GetSelf());
            var seed = await seeder.Seed();
            _currentSeed = Some(seed);
        }

        private void ResetSeed()
        {
            _currentSeed = None;
        }

        protected abstract WebServiceFixtureSeeder<TSelf, TSeed> CreateSeeder(TSelf fixture);

        public abstract class FollowingTestLifetime : WithSeeding<TSeed>
        {
            protected FollowingTestLifetime(Type entryPointMarker) : base(entryPointMarker)
            {
            }

            internal override async Task BeforeTestInternal()
            {
                await ApplySeed();
                await base.BeforeTestInternal();
            }

            internal override async Task ResetInternal()
            {
                await base.ResetInternal();
                ResetSeed();
            }
        }

        public abstract class FollowingFixtureLifetime : WithSeeding<TSeed>
        {
            protected FollowingFixtureLifetime(Type entryPointMarker) : base(entryPointMarker)
            {
            }

            internal override async Task InitializeInternal()
            {
                await ApplySeed();
                await base.InitializeInternal();
            }

            internal override async Task DisposeInternal()
            {
                await base.DisposeInternal();
                ResetSeed();
            }
        }
    }
}
