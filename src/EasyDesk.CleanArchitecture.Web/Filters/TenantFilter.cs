using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Infrastructure.Tenants;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public class TenantFilter : IAsyncActionFilter
{
    private readonly ITenantInitializer _tenantInitializer;

    public TenantFilter(ITenantInitializer tenantInitializer)
    {
        _tenantInitializer = tenantInitializer;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantId = context.HttpContext.GetTenantId();
        _tenantInitializer.InitializeTenant(tenantId);
        await next();
    }
}
