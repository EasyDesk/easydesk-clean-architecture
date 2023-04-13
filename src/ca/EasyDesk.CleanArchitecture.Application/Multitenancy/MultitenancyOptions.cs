namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public sealed class MultitenancyOptions
{
    public MultitenantPolicy DefaultPolicy { get; set; } = MultitenantPolicies.IgnoreAndUsePublic();

    public Func<IServiceProvider, IContextTenantReader> TenantReaderImplementation { get; set; } =
        _ => new PublicContextTenantReader();
}
