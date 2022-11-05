using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class DefaultTenantProvider : ITenantProvider
{
    public const string TenantIdHttpHeader = "x-tenant-id";
    public const string TenantIdHttpQueryParam = "tenantId";

    private readonly Lazy<Option<string>> _tenantId;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultTenantProvider(IContextProvider contextProvider, IHttpContextAccessor httpContextAccessor)
    {
        _tenantId = new Lazy<Option<string>>(() => contextProvider.Context switch
        {
            RequestContext => GetTenantIdForRequestContext(),
            AsyncMessageContext => GetTenantIdForAsyncMessageContext(),
            _ => None
        });
        _httpContextAccessor = httpContextAccessor;
    }

    public Option<string> TenantId => _tenantId.Value;

    private Option<string> GetTenantIdForRequestContext()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return request.Headers[TenantIdHttpHeader].FirstOption()
            | request.Query[TenantIdHttpQueryParam].FirstOption();
    }

    private Option<string> GetTenantIdForAsyncMessageContext() =>
        MessageContext.Current.Headers.GetOption(MultitenantUtils.TenantIdHeader);
}
