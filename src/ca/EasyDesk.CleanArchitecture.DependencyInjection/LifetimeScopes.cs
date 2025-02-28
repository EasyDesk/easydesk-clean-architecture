using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace EasyDesk.CleanArchitecture.DependencyInjection;

public static class LifetimeScopes
{
    public static object UseCaseScopeTag { get; } = "__use_case_scope__";

    public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerUseCase<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
        params object[] lifetimeScopeTags)
    {
        return registration.InstancePerMatchingOrMostNestedLifetimeScope([.. lifetimeScopeTags, UseCaseScopeTag]);
    }

    public static ILifetimeScope BeginUseCaseLifetimeScope(this ILifetimeScope scope) =>
        scope.BeginLifetimeScope(UseCaseScopeTag);

    public static ILifetimeScope BeginUseCaseLifetimeScope(this ILifetimeScope scope, Action<ContainerBuilder> configurationAction) =>
        scope.BeginLifetimeScope(UseCaseScopeTag, configurationAction);

    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingOrRootLifetimeScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, params object[] lifetimeScopeTag)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScopeTag);
        builder.RegistrationData.Sharing = InstanceSharing.Shared;
        builder.RegistrationData.Lifetime = new MatchingScopeOrFallbackLifetime(scope => scope.RootLifetimeScope, lifetimeScopeTag);
        return builder;
    }

    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingOrMostNestedLifetimeScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, params object[] lifetimeScopeTag)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScopeTag);

        builder.RegistrationData.Sharing = InstanceSharing.Shared;
        builder.RegistrationData.Lifetime = new MatchingScopeOrFallbackLifetime(It, lifetimeScopeTag);
        return builder;
    }
}

internal class MatchingScopeOrFallbackLifetime : IComponentLifetime
{
    private readonly Func<ISharingLifetimeScope, ISharingLifetimeScope> _fallbackScope;
    private readonly object[] _tagsToMatch;

    public MatchingScopeOrFallbackLifetime(
        Func<ISharingLifetimeScope, ISharingLifetimeScope> fallbackScope,
        params object[] lifetimeScopeTagsToMatch)
    {
        _fallbackScope = fallbackScope;
        _tagsToMatch = lifetimeScopeTagsToMatch ?? throw new ArgumentNullException(nameof(lifetimeScopeTagsToMatch));
    }

    public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
    {
        ArgumentNullException.ThrowIfNull(mostNestedVisibleScope);

        var next = mostNestedVisibleScope;
        while (next != null)
        {
            if (_tagsToMatch.Contains(next.Tag))
            {
                return next;
            }

            next = next.ParentLifetimeScope;
        }

        return _fallbackScope(mostNestedVisibleScope);
    }
}
