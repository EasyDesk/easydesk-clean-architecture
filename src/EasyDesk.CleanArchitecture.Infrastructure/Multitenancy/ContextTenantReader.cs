using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public static class MultitenancyDefaults
{
    public const string TenantIdHttpHeader = "x-tenant-id";
    public const string TenantIdHttpQueryParam = "tenantId";
}

internal class ContextTenantReader
{
    private readonly IContextProvider _contextProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextTenantReader(IContextProvider contextProvider, IHttpContextAccessor httpContextAccessor)
    {
        _contextProvider = contextProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public Option<string> GetTenantId() => _contextProvider.Context switch
    {
        RequestContext => GetTenantIdForRequestContext(),
        AsyncMessageContext => GetTenantIdForAsyncMessageContext(),
        _ => None
    };

    private Option<string> GetTenantIdForRequestContext()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return request.Headers[MultitenancyDefaults.TenantIdHttpHeader].FirstOption()
            | request.Query[MultitenancyDefaults.TenantIdHttpQueryParam].FirstOption();
    }

    private Option<string> GetTenantIdForAsyncMessageContext() =>
        MessageContext.Current.Headers.GetOption(MultitenantUtils.TenantIdHeader);
}
