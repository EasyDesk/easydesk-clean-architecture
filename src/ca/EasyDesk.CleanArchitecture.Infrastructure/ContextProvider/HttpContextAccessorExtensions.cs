using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public static class HttpContextAccessorExtensions
{
    public static IHttpContextAccessor Setup(this IHttpContextAccessor httpContextAccessor, Action<HttpContext> configure)
    {
        var httpContext = httpContextAccessor.HttpContext.AsOption().OrElseGet(() => new DefaultHttpContext());
        configure.Invoke(httpContext);
        httpContextAccessor.HttpContext = httpContext;
        return httpContextAccessor;
    }

    public static HttpContext SetupTenant(this HttpContext httpContext, TenantId tenantId)
    {
        httpContext.Request.Headers[MultitenancyDefaults.TenantIdHttpHeader] = tenantId.Value;
        return httpContext;
    }

    public static HttpContext SetupAuthenticatedUser(this HttpContext httpContext, string userId)
    {
        httpContext.User = new(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            "Custom"));
        return httpContext;
    }
}
