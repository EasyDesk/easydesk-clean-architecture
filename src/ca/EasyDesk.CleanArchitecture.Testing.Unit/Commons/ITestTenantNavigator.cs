using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Commons;

public interface ITestTenantNavigator : ITenantNavigator
{
    bool IsMultitenancyIgnored { get; }

    void IgnoreMultitenancy();
}

public static class TestTenantNavigatorExtensions
{
    public static void MoveToOrIgnore(this ITestTenantNavigator tenantNavigator, Option<TenantInfo> tenantInfo) =>
        tenantInfo.Match(some: tenantNavigator.MoveTo, none: tenantNavigator.IgnoreMultitenancy);
}
