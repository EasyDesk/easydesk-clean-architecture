using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantProvider
{
    Option<string> TenantId { get; }
}

public static class TenantProviderExtensions
{
    public static bool IsInTenant(this ITenantProvider tenantProvider) => tenantProvider.TenantId.IsPresent;
}
