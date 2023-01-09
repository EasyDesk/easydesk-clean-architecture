using EasyDesk.CleanArchitecture.Testing.Integration.Commons.Polling;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class InjectedServiceCheckFactory<TService>
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(10);
    private static readonly Duration _defaultQueryInterval = Duration.FromMilliseconds(200);

    public static async Task SingleScopeUntil(
        IServiceProvider serviceProvider,
        AsyncFunc<TService, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var polling = new Polling<TService>(
                _ => Task.FromResult(scope.ServiceProvider.GetRequiredService<TService>()),
                timeout ?? _defaultPollTimeout,
                interval ?? _defaultQueryInterval);
            await polling.PollUntil(predicate);
        }
    }

    public static async Task ScopedUntil(
        IServiceProvider serviceProvider,
        AsyncFunc<TService, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
    {
        var polling = new Polling<bool>(
            async token =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    return await predicate(service);
                }
            },
            timeout ?? _defaultPollTimeout,
            interval ?? _defaultQueryInterval);
        await polling.PollUntil(It);
    }
}
