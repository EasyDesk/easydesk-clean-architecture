namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public enum MultitenantPolicy
{
    AllowAll,
    RequireTenant,
    RequireNoTenant
}

public class MultitenancyOptions
{
    public MultitenantPolicy DefaultPolicy { get; set; } = MultitenantPolicy.AllowAll;

    public Func<IServiceProvider, ITenantProvider> TenantProviderFactory { get; set; }
}
