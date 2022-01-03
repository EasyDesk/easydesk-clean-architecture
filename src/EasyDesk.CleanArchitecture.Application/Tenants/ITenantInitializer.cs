using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.Tenants;

public interface ITenantInitializer
{
    public void InitializeTenant(Option<string> tenantId);
}
