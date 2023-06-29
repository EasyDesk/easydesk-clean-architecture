using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;

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
        httpContext.Request.Headers[CommonTenantReaders.TenantIdHttpHeader] = tenantId.Value;
        return httpContext;
    }

    public static HttpContext SetupAuthenticatedAgent(this HttpContext httpContext, Agent agent)
    {
        httpContext.User = new(agent.ToClaimsIdentity());
        return httpContext;
    }
}
