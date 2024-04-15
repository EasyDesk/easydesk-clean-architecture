using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class EfCoreMultitenancyManager : IMultitenancyManager
{
    private readonly AuthorizationContext _context;

    public EfCoreMultitenancyManager(AuthorizationContext context)
    {
        _context = context;
    }

    public async Task AddTenant(TenantId tenantId)
    {
        _context.Tenants.Add(new TenantModel
        {
            Id = tenantId,
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveTenant(TenantId tenantId)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == tenantId)
            .FirstOptionAsync();

        tenant.IfPresent(t => _context.Tenants.Remove(t));

        await _context.SaveChangesAsync();
    }

    public async Task<bool> TenantExists(TenantId tenantId)
    {
        return await _context.Tenants.AnyAsync(t => t.Id == tenantId);
    }
}
