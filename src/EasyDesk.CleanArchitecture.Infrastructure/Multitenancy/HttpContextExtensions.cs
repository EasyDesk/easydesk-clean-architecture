using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Http;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public static class HttpContextExtensions
{
    public const string TenantIdClaimName = "tenantId";

    public static Option<string> GetTenantId(this HttpContext httpContext) =>
        httpContext.User.FindFirst(TenantIdClaimName).AsOption().Map(c => c.Value);
}
