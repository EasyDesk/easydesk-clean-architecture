using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public static class SeedingFixtureExtensions
{
    public static TestFixtureConfigurer AddSeedingOnInitialization<TFixture, TSeed>(this TestFixtureConfigurer configurer, ISeeder<TFixture, TSeed> seeder)
        where TFixture : IntegrationTestsFixture
    {
        configurer.ContainerBuilder
            .RegisterType<SeedHolder<TSeed>>()
            .SingleInstance();

        return configurer.RegisterLifetimeHooks(c => new SeedingOnInitializationLifetimeHooks<TFixture, TSeed>(
            c.Resolve<SeedHolder<TSeed>>(),
            c.Resolve<TFixture>(),
            seeder));
    }

    internal class SeedingOnInitializationLifetimeHooks<TFixture, TSeed> : LifetimeHooks
        where TFixture : IntegrationTestsFixture
    {
        private readonly SeedHolder<TSeed> _seedHolder;
        private readonly TFixture _fixture;
        private readonly ISeeder<TFixture, TSeed> _seeder;

        public SeedingOnInitializationLifetimeHooks(SeedHolder<TSeed> seedHolder, TFixture fixture, ISeeder<TFixture, TSeed> seeder)
        {
            _seedHolder = seedHolder;
            _fixture = fixture;
            _seeder = seeder;
        }

        public override async Task OnInitialization()
        {
            await using var session = new IntegrationTestSession<TFixture>(_fixture, _seeder.ConfigureSession);
            var seed = await _seeder.Seed(session);
            _seedHolder.StoreSeed(seed);
        }

        public override Task OnDisposal()
        {
            _seedHolder.ClearSeed();
            return Task.CompletedTask;
        }
    }

    public static TSeed GetSeed<TSeed>(this IntegrationTestsFixture fixture) => fixture.Container.Resolve<SeedHolder<TSeed>>().Seed;
}
