namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantNavigator : ITenantProvider
{
    IDisposable NavigateTo(TenantInfo tenantInfo);
}

public static class TenantNavigatorExtensions
{
    public static IDisposable NavigateToTenant(this ITenantNavigator navigator, TenantId tenantId) =>
        navigator.NavigateTo(TenantInfo.Tenant(tenantId));

    public static IDisposable NavigateToPublic(this ITenantNavigator navigator) =>
        navigator.NavigateTo(TenantInfo.Public);
}
