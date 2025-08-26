using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixture;

public abstract class IntegrationTestsFixture : IAsyncLifetime
{
    protected IntegrationTestsFixture()
    {
        var configurer = new TestFixtureConfigurer();

        configurer.ContainerBuilder.RegisterInstance(this)
            .AsParentsUpTo(leaf: GetType(), root: typeof(IntegrationTestsFixture))
            .SingleInstance()
            .ExternallyOwned();

        ConfigureFixture(configurer);

        Container = configurer.BuildContainer();
    }

    public IContainer Container { get; }

    protected abstract void ConfigureFixture(TestFixtureConfigurer configurer);

    public async ValueTask InitializeAsync()
    {
        await TriggerFixtureLifetimeHook(l => l.OnInitialization());
    }

    public async Task BeforeTest()
    {
        await TriggerFixtureLifetimeHook(l => l.BeforeTest());
    }

    public async Task AfterTest()
    {
        await TriggerFixtureLifetimeHook(l => l.AfterTest(), reverseOrder: true);

        await Pause();
        await TriggerFixtureLifetimeHook(l => l.BetweenTests());
        await Resume();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
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
