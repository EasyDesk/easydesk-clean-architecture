namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class MultitenancyOptions
{
    public MultitenantPolicy DefaultPolicy { get; set; } = MultitenantPolicies.Public();

    public Func<IServiceProvider, IContextTenantReader> TenantReaderImplementation { get; set; } =
        _ => new PublicContextTenantReader();
}
