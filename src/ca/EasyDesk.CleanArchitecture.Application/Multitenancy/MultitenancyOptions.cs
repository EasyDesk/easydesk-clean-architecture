namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public sealed class MultitenancyOptions
{
    public MultitenantPolicy DefaultPolicy { get; private set; } = MultitenantPolicies.IgnoreAndUsePublic();

    public MultitenancyOptions WithDefaultPolicy(MultitenantPolicy policy)
    {
        DefaultPolicy = policy;
        return this;
    }
}
