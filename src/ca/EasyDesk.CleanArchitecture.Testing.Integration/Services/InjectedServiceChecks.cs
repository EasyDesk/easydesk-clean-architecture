using EasyDesk.CleanArchitecture.Testing.Integration.Polling;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class InjectedServiceChecks
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(10);
    private static readonly Duration _defaultQueryInterval = Duration.FromMilliseconds(200);

    public static async Task SingleScopePollUntil<T>(
        this IServiceProvider serviceProvider,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var polling = Poll
                .Sync(scope.ServiceProvider.GetRequiredService<T>)
                .WithTimeout(timeout ?? _defaultPollTimeout)
                .WithInterval(interval ?? _defaultQueryInterval);
            await polling.Until(predicate);
        }
    }

    public static async Task ScopedPollUntil<T>(
        this IServiceProvider serviceProvider,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        var polling = Poll
            .Async(async token =>
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<T>();
                    return await predicate(service);
                }
            })
            .WithTimeout(timeout ?? _defaultPollTimeout)
            .WithInterval(interval ?? _defaultQueryInterval);
        await polling.Until(It);
    }
}
