using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IContextTenantNavigator : ITenantNavigator
{
    Option<TenantInfo> ContextTenant { get; }
}
