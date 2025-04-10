using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public static class TestContainerExtensions
{
    public static TSelf WithUniqueName<TSelf, TContainer>(this IContainerBuilder<TSelf, TContainer> builder, string name)
        where TSelf : IContainerBuilder<TSelf, TContainer> =>
        builder.WithName($"{name}-{Guid.NewGuid().ToString()[..8]}");

    public static TestFixtureConfigurer RegisterDockerContainer<T>(this TestFixtureConfigurer configurer, T container) where T : IContainer
    {
        return configurer.RegisterLifetimeHooks(
            onInitialization: async () => await container.StartAsync(),
            onDisposal: async () => await container.DisposeAsync());
    }
}
