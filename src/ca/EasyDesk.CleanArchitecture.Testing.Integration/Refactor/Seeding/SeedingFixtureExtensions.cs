using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Seeding;

public static class SeedingFixtureExtensions
{
    public static TestFixtureConfigurer AddSeedingOnInitialization<TFixture, TSeed>(this TestFixtureConfigurer configurer, ISeeder<TFixture, TSeed> seeder)
        where TFixture : IntegrationTestsFixture
    {
        configurer.ContainerBuilder
            .Register(c => new SeedManager<TFixture, TSeed>(seeder, c.Resolve<TFixture>()))
            .SingleInstance();

        return configurer.RegisterLifetimeHooks<SeedingOnInitializationLifetimeHooks<TFixture, TSeed>>();
    }

    internal class SeedingOnInitializationLifetimeHooks<TFixture, TSeed> : LifetimeHooks
        where TFixture : IntegrationTestsFixture
    {
        private readonly SeedManager<TFixture, TSeed> _seedManager;

        public SeedingOnInitializationLifetimeHooks(SeedManager<TFixture, TSeed> seedManager)
        {
            _seedManager = seedManager;
        }

        public override async Task OnInitialization()
        {
            await _seedManager.ApplySeed();
        }

        public override Task OnDisposal()
        {
            _seedManager.ClearSeed();
            return Task.CompletedTask;
        }
    }
}
