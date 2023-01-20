using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public static class HttpContextAccessorExtensions
{
    public static void SetupAuthenticatedHttpContext(this IHttpContextAccessor httpContextAccessor, string userId)
    {
        var httpContext = new DefaultHttpContext();
        httpContextAccessor.HttpContext.AsOption().IfPresent(c => httpContext.Initialize(c.Features));
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                "Custom"));
        httpContextAccessor.HttpContext = httpContext;
    }
}
