namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IMultitenancyManager
{
    Task AddTenant(TenantId tenantId);

    Task RemoveTenant(TenantId tenantId);

    Task<bool> TenantExists(TenantId tenantId);
}
