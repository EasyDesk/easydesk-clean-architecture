namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IMultitenancyManager
{
    Task AddTenant(string tenantId);

    Task RemoveTenant(string tenantId);

    Task<bool> TenantExists(string tenantId);
}
