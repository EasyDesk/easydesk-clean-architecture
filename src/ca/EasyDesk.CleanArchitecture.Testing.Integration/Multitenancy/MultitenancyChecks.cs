using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;

public static class MultitenancyChecks
{
    public static Task WaitUntilTenantExists(
        this ITestHost host, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        host.LifetimeScope.ScopedPollUntil<IMultitenancyManager>(
            m => m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);

    public static Task WaitUntilTenantDoesNotExist(
        this ITestHost host, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        host.LifetimeScope.ScopedPollUntil<IMultitenancyManager>(
            async m => !await m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);
}
