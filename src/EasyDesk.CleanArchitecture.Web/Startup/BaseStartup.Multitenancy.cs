using EasyDesk.CleanArchitecture.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    protected abstract bool IsMultitenant { get; }

    private void AddMultitenancySupport(IServiceCollection services)
    {
        if (!IsMultitenant)
        {
            return;
        }
        services.AddTenantManagement();
    }
}
