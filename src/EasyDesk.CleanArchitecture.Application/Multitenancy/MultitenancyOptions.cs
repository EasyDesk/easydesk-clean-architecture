namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class MultitenancyOptions
{
    public MultitenantPolicy DefaultPolicy { get; set; } = MultitenantPolicy.AllowAll;
}

public enum MultitenantPolicy
{
    AllowAll,
    RequireTenant,
    RequireNoTenant
}
