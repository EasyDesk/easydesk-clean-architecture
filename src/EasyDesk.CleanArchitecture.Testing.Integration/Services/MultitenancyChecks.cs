using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class MultitenancyChecks
{
    public static Task WaitUntilTenantExists(this ITestWebService webService, TenantId tenantId) =>
        InjectedServiceCheckFactory<IMultitenancyManager>.SingleScopeUntil(webService.Services, m => m.TenantExists(tenantId));
}
