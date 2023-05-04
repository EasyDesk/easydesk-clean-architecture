using DotNet.Testcontainers.Builders;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public static class TestContainerExtensions
{
    public static TSelf WithUniqueName<TSelf, TContainer>(this IContainerBuilder<TSelf, TContainer> builder, string name)
        where TSelf : IContainerBuilder<TSelf, TContainer> =>
        builder.WithName($"{name}-{Guid.NewGuid().ToString()[..8]}");
}
