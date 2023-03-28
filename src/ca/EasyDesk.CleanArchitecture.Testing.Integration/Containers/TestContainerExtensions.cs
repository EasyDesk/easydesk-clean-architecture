using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Containers;

public static class TestContainerExtensions
{
    public static ITestcontainersBuilder<T> WithUniqueName<T>(this ITestcontainersBuilder<T> builder, string name)
        where T : TestcontainersContainer => builder.WithName($"{name}-{Guid.NewGuid().ToString()[..8]}");
}
