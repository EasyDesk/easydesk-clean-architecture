using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class MultitenantContextResetter : IContextResetter
{
    private readonly IContextResetter _contextResetter;
    private readonly IContextTenantNavigator _tenantNavigator;

    public MultitenantContextResetter(IContextResetter contextResetter, IContextTenantNavigator tenantNavigator)
    {
        _contextResetter = contextResetter;
        _tenantNavigator = tenantNavigator;
    }

    public void ResetContext()
    {
        _tenantNavigator.MoveToContextTenant();
        _contextResetter.ResetContext();
    }
}
