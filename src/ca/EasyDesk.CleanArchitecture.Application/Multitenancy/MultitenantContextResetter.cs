using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class MultitenantContextResetter : IContextResetter
{
    private readonly IContextResetter _contextResetter;
    private readonly ITenantNavigator _tenantNavigator;

    public MultitenantContextResetter(IContextResetter contextResetter, ITenantNavigator tenantNavigator)
    {
        _contextResetter = contextResetter;
        _tenantNavigator = tenantNavigator;
    }

    public async Task ResetContext()
    {
        _tenantNavigator.MoveToContextTenant();
        await _contextResetter.ResetContext();
    }
}
