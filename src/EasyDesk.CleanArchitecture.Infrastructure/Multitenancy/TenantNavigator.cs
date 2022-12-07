using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

internal class TenantNavigator : ITenantNavigator
{
    private Option<Option<string>> _tenantIdOverride = None;
    private readonly Lazy<Option<string>> _contextTenantId;

    public TenantNavigator(ContextTenantReader contextTenantReader)
    {
        _contextTenantId = new(contextTenantReader.GetTenantId);
    }

    public void MoveTo(string tenantId)
    {
        _tenantIdOverride = Some(Some(tenantId));
    }

    public void MoveToNoTenant()
    {
        _tenantIdOverride = Some<Option<string>>(None);
    }

    public void BackToContextTenant()
    {
        _tenantIdOverride = None;
    }

    public Option<string> TenantId => _tenantIdOverride.OrElseGet(() => _contextTenantId.Value);
}
