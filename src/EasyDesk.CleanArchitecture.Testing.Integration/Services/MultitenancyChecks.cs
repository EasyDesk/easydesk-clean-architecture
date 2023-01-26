using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class MultitenancyChecks
{
    public static Task WaitUntilTenantExists(
        this ITestWebService webService, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        InjectedServiceCheckFactory<IMultitenancyManager>.ScopedUntil(
            webService.Services,
            m => m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);

    public static Task WaitUntilTenantDoesNotExist(
        this ITestWebService webService, TenantId tenantId, Duration? timeout = null, Duration? interval = null) =>
        InjectedServiceCheckFactory<IMultitenancyManager>.ScopedUntil(
            webService.Services,
            async m => !await m.TenantExists(tenantId),
            timeout: timeout,
            interval: interval);

    public static Task WaitConditionUnderTenant<TService>(
        this ITestWebService webService,
        TenantId tenantId,
        AsyncFunc<TService, bool> condition,
        Duration? timeout = null,
        Duration? interval = null)
        where TService : notnull =>
        InjectedServiceCheckFactory<IServiceProvider>.ScopedUntil(
            webService.Services,
            async services =>
            {
                services.GetRequiredService<IContextTenantInitializer>()
                        .Initialize(TenantInfo.Tenant(tenantId));
                var service = services.GetRequiredService<TService>();
                return await condition(service);
            },
            timeout: timeout,
            interval: interval);
}
