using EasyDesk.Commons.Tasks;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Lifetime;

public static class AsyncLifetime
{
    public static async Task UsingAsyncLifetime<T>(Func<T> factory, AsyncAction<T> action)
        where T : IAsyncLifetime
    {
        await using var asyncLifetime = factory();
        await asyncLifetime.InitializeAsync();
        await action(asyncLifetime);
    }
}
