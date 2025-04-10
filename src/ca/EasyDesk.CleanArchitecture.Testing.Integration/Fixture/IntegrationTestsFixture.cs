using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixture;

public abstract class IntegrationTestsFixture : IAsyncLifetime
{
    private bool _firstBeforeTestRun = false;

    protected IntegrationTestsFixture()
    {
        var configurer = new TestFixtureConfigurer();

        configurer.ContainerBuilder.RegisterInstance(this)
            .As(GetType())
            .SingleInstance();

        ConfigureFixture(configurer);

        Container = configurer.BuildContainer();
    }

    public IContainer Container { get; }

    protected abstract void ConfigureFixture(TestFixtureConfigurer configurer);

    public async Task InitializeAsync()
    {
        await TriggerFixtureLifetimeHook(l => l.OnInitialization());
    }

    public async Task BeforeTest()
    {
        if (_firstBeforeTestRun)
        {
            await Pause();
            await TriggerFixtureLifetimeHook(l => l.BetweenTests());
            await Resume();
        }
        else
        {
            _firstBeforeTestRun = true;
        }

        await TriggerFixtureLifetimeHook(l => l.BeforeTest());
    }

    public async Task AfterTest()
    {
        await TriggerFixtureLifetimeHook(l => l.AfterTest(), reverseOrder: true);
    }

    public async Task DisposeAsync()
    {
        await TriggerFixtureLifetimeHook(l => l.OnDisposal(), reverseOrder: true);
        await Container.DisposeAsync();
    }

    private async Task Pause()
    {
        foreach (var pausable in Container.Resolve<IEnumerable<IPausable>>())
        {
            await pausable.Pause(CancellationToken.None);
        }
    }

    private async Task Resume()
    {
        foreach (var pausable in Container.Resolve<IEnumerable<IPausable>>())
        {
            await pausable.Resume(CancellationToken.None);
        }
    }

    private async Task TriggerFixtureLifetimeHook(AsyncAction<IFixtureLifetime> hook, bool reverseOrder = false)
    {
        var fixtureLifetimes = Container.Resolve<IEnumerable<IFixtureLifetime>>();

        if (reverseOrder)
        {
            fixtureLifetimes = fixtureLifetimes.Reverse();
        }

        foreach (var lifetime in fixtureLifetimes)
        {
            await hook(lifetime);
        }
    }
}
