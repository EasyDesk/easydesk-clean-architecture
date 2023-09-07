using EasyDesk.CleanArchitecture.Testing.Integration.Polling;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class InjectedServiceCheckFactory<TService>
    where TService : notnull
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(10);
    private static readonly Duration _defaultQueryInterval = Duration.FromMilliseconds(200);

    public static async Task SingleScopeUntil(
        IServiceProvider serviceProvider,
        AsyncFunc<TService, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var polling = Poll
                .Sync(scope.ServiceProvider.GetRequiredService<TService>)
                .WithTimeout(timeout ?? _defaultPollTimeout)
                .WithInterval(interval ?? _defaultQueryInterval);
            await polling.Until(predicate);
        }
    }

    public static async Task ScopedUntil(
        IServiceProvider serviceProvider,
        AsyncFunc<TService, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
    {
        var polling = Poll
            .Async(async token =>
            {
                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    return await predicate(service);
                }
            })
            .WithTimeout(timeout ?? _defaultPollTimeout)
            .WithInterval(interval ?? _defaultQueryInterval);
        await polling.Until(It);
    }
}
