using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class OverridableContextProvider : IContextProvider
{
    private readonly IContextProvider _defaultContextProvider;
    private Option<ContextInfo> _overriddenContextInfo;
    private Option<Option<string>> _overriddenTenantId;
    private Option<CancellationToken> _overriddenCancellationToken;

    public OverridableContextProvider(IContextProvider defaultContextProvider)
    {
        _defaultContextProvider = defaultContextProvider;
    }

    public ContextInfo CurrentContext => _overriddenContextInfo.OrElseGet(() => _defaultContextProvider.CurrentContext);

    public Option<string> TenantId => _overriddenTenantId.OrElseGet(() => _defaultContextProvider.TenantId);

    public CancellationToken CancellationToken => _overriddenCancellationToken.OrElseGet(() => _defaultContextProvider.CancellationToken);

    public void OverrideContextInfo(ContextInfo contextInfo)
    {
        if (_overriddenContextInfo.IsPresent)
        {
            throw new InvalidOperationException("Default context info was already overridden in this scope.");
        }
        _overriddenContextInfo = Some(contextInfo);
    }

    public void OverrideTenantId(Option<string> tenantId)
    {
        if (_overriddenTenantId.IsPresent)
        {
            throw new InvalidOperationException("Default tenant info was already overridden in this scope.");
        }
        _overriddenTenantId = Some(tenantId);
    }

    public void OverrideCancellationToken(CancellationToken cancellationToken)
    {
        if (_overriddenCancellationToken.IsPresent)
        {
            throw new InvalidOperationException("Default cancellation token was already overridden in this scope.");
        }
        _overriddenCancellationToken = Some(cancellationToken);
    }
}
