using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public static class HttpContextAccessorExtensions
{
    public static IHttpContextAccessor SetupAuthenticatedHttpContext(this IHttpContextAccessor httpContextAccessor, string userId)
    {
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.AsOption().IfPresent(c => httpContext.Initialize(c.Features));
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                "Custom"));
        httpContextAccessor.HttpContext = httpContext;
        return httpContextAccessor;
    }

    public static IHttpContextAccessor SetupMultitenantHttpContext(this IHttpContextAccessor httpContextAccessor, string tenantId)
    {
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.AsOption().IfPresent(c => httpContext.Initialize(c.Features));
        httpContext.Request.Headers[MultitenancyDefaults.TenantIdHttpHeader] = tenantId;
        httpContextAccessor.HttpContext = httpContext;
        return httpContextAccessor;
    }
}
