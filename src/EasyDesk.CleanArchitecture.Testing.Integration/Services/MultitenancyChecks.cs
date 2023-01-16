using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;

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
}
