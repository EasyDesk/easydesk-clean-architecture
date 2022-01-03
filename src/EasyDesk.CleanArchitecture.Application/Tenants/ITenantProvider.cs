using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.Tenants;

public interface ITenantProvider
{
    Option<string> TenantId { get; }
}

public static class TenantProviderExtensions
{
    public static bool IsInTenant(this ITenantProvider tenantProvider) => tenantProvider.TenantId.IsPresent;
}
