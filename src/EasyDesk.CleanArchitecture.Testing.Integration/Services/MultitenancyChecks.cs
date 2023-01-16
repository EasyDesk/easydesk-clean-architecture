using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class MultitenancyChecks
{
    public static Task WaitUntilTenantExists(this ITestWebService webService, TenantId tenantId) =>
        InjectedServiceCheckFactory<IMultitenancyManager>.ScopedUntil(
            webService.Services,
            m => m.TenantExists(tenantId));

    public static Task WaitUntilTenantDoesNotExist(this ITestWebService webService, TenantId tenantId) =>
        InjectedServiceCheckFactory<IMultitenancyManager>.ScopedUntil(
            webService.Services,
            async m => !await m.TenantExists(tenantId));

    public static Task WaitConditionUnderTenant<TService>(this ITestWebService webService, TenantId tenantId, AsyncFunc<TService, bool> condition) =>
        InjectedServiceCheckFactory<IServiceProvider>.ScopedUntil(
            webService.Services,
            async services =>
            {
                services.GetRequiredService<IContextTenantInitializer>()
                        .Initialize(TenantInfo.Tenant(tenantId));
                var service = services.GetRequiredService<TService>();
                return await condition(service);
            });
}
