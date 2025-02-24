using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class MultitenancyChecks
{
    public static Task WaitUntilTenantExists(
        this ITestWebService webService, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        webService.LifetimeScope.ScopedPollUntil<IMultitenancyManager>(
            m => m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);

    public static Task WaitUntilTenantDoesNotExist(
        this ITestWebService webService, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        webService.LifetimeScope.ScopedPollUntil<IMultitenancyManager>(
            async m => !await m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);
}
