using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public static class MultitenancyChecks
{
    public static async Task WaitUntilTenantExists(this ITestWebService webService, TenantId tenantId)
    {
        var tenantChecker = new InjectedServiceCheckBuilder<IMultitenancyManager>(webService.Services);
        await tenantChecker.WaitUntil(m => m.TenantExists(tenantId));
    }
}
