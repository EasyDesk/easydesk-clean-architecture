using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Testing.Integration.Polling;
using EasyDesk.Commons.Tasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class InjectedServiceChecks
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(10);
    private static readonly Duration _defaultQueryInterval = Duration.FromMilliseconds(200);

    public static async Task SingleScopePollUntil<T>(
        this ILifetimeScope lifetimeScope,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        await using (var scope = lifetimeScope.BeginUseCaseLifetimeScope())
        {
            var polling = Poll
                .Sync(scope.Resolve<T>)
                .WithTimeout(timeout ?? _defaultPollTimeout)
                .WithInterval(interval ?? _defaultQueryInterval);
            await polling.Until(predicate);
        }
    }

    public static async Task ScopedPollUntil<T>(
        this ILifetimeScope lifetimeScope,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        var polling = Poll
            .Async(async token =>
            {
                await using (var scope = lifetimeScope.BeginUseCaseLifetimeScope())
                {
                    var service = scope.Resolve<T>();
                    return await predicate(service);
                }
            })
            .WithTimeout(timeout ?? _defaultPollTimeout)
            .WithInterval(interval ?? _defaultQueryInterval);
        await polling.Until(It);
    }

    public static async Task SingleScopePollInvariant<T>(
        this ILifetimeScope lifetimeScope,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        await using (var scope = lifetimeScope.BeginUseCaseLifetimeScope())
        {
            var polling = Poll
                .Sync(scope.Resolve<T>)
                .WithTimeout(timeout ?? _defaultPollTimeout)
                .WithInterval(interval ?? _defaultQueryInterval);
            await polling.EnsureInvariant(predicate);
        }
    }

    public static async Task ScopedPollInvariant<T>(
        this ILifetimeScope lifetimeScope,
        AsyncFunc<T, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
        where T : notnull
    {
        var polling = Poll
            .Async(async token =>
            {
                await using (var scope = lifetimeScope.BeginUseCaseLifetimeScope())
                {
                    var service = scope.Resolve<T>();
                    return await predicate(service);
                }
            })
            .WithTimeout(timeout ?? _defaultPollTimeout)
            .WithInterval(interval ?? _defaultQueryInterval);
        await polling.EnsureInvariant(It);
    }
}
