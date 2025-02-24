using Autofac;
using Autofac.Builder;

namespace EasyDesk.CleanArchitecture.DependencyInjection;

public static class LifetimeScopes
{
    public static object UseCaseScopeTag { get; } = "__use_case_scope__";

    public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerUseCase<TLimit, TActivatorData, TStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
        params object[] lifetimeScopeTags)
    {
        return registration.InstancePerMatchingLifetimeScope([.. lifetimeScopeTags, UseCaseScopeTag]);
    }

    public static ILifetimeScope BeginUseCaseLifetimeScope(this ILifetimeScope scope) =>
        scope.BeginLifetimeScope(UseCaseScopeTag);

    public static ILifetimeScope BeginUseCaseLifetimeScope(this ILifetimeScope scope, Action<ContainerBuilder> configurationAction) =>
        scope.BeginLifetimeScope(UseCaseScopeTag, configurationAction);
}
