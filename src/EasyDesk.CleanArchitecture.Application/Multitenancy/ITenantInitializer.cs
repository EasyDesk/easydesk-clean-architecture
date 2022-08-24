namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantInitializer
{
    public void InitializeTenant(Option<string> tenantId);
}
