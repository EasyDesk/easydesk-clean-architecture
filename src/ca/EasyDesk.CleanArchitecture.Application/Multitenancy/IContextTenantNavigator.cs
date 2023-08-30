using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IContextTenantNavigator : ITenantNavigator
{
    Option<TenantInfo> ContextTenant { get; }

    void MoveToContextTenant();
}

public static class ContextTenantNavigatorExtensions
{
    public static void MoveTo(this IContextTenantNavigator navigator, Option<TenantInfo> tenantInfo) =>
        tenantInfo.Match(some: navigator.MoveTo, none: navigator.MoveToContextTenant);
}
