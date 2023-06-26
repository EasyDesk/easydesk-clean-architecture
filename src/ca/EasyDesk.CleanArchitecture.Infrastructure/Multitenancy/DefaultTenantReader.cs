using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public static class MultitenancyDefaults
{
    public const string TenantIdHttpHeader = "x-tenant-id";
    public const string TenantIdHttpQueryParam = "tenantId";
}

internal class DefaultTenantReader : ITenantReader
{
    public Option<string> ReadFromHttpContext(HttpContext httpContext) =>
        ReadTenantIdFromHeaders(httpContext.Request) | ReadTenantIdFromQuery(httpContext.Request);

    private static Option<string> ReadTenantIdFromHeaders(HttpRequest request) =>
        request.Headers[MultitenancyDefaults.TenantIdHttpHeader].WhereNotNull().FirstOption();

    private static Option<string> ReadTenantIdFromQuery(HttpRequest request) =>
        request.Query[MultitenancyDefaults.TenantIdHttpQueryParam].SelectMany(h => h.AsOption()).FirstOption();

    public Option<string> ReadFromMessageContext(IMessageContext messageContext) =>
        messageContext.Headers.GetOption(MultitenantMessagingUtils.TenantIdHeader);
}
