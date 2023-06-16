using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class EfCoreAuthorizationManager : IAgentPermissionsProvider, IIdentityRolesManager, IAgentRolesProvider, IRolesToPermissionsMapper
{
    private readonly AuthorizationContext _context;

    public EfCoreAuthorizationManager(AuthorizationContext context)
    {
        _context = context;
    }

    private IQueryable<IdentityRoleModel> RolesByAgent(Agent agent)
    {
        var identities = agent.Identities.Values.Select(x => x.Id.Value);
        return _context
            .IdentityRoles
            .Where(u => identities.Contains(u.Identity));
    }

    private IQueryable<IdentityRoleModel> RolesByIdentity(IdentityId id)
    {
        return _context
            .IdentityRoles
            .Where(u => u.Identity == id);
    }

    public async Task<IImmutableSet<Role>> GetRolesForAgent(Agent agent)
    {
        return await RolesByAgent(agent)
            .Select(u => new Role(u.Role))
            .ToEquatableSetAsync();
    }

    public async Task GrantRolesToIdentity(IdentityId identityId, IEnumerable<Role> roles)
    {
        var currentRoleIds = await RolesByIdentity(identityId)
            .Select(r => r.Role)
            .ToListAsync();

        var rolesToBeAdded = RoleIds(roles)
            .Except(currentRoleIds)
            .Select(role => IdentityRoleModel.Create(identityId, role));

        _context.IdentityRoles.AddRange(rolesToBeAdded);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRolesToIdentity(IdentityId id, IEnumerable<Role> roles)
    {
        var roleIds = RoleIds(roles);
        var rolesToBeRemoved = await RolesByIdentity(id)
            .Where(x => roleIds.Contains(x.Role))
            .ToListAsync();

        _context.IdentityRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllRolesToIdentity(IdentityId id)
    {
        var rolesToBeRemoved = await RolesByIdentity(id)
            .ToListAsync();

        _context.IdentityRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    private IEnumerable<string> RoleIds(IEnumerable<Role> roles) => roles.Select(ValueWrapperUtils.ToValue);

    public async Task<IImmutableSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles)
    {
        var roleIds = RoleIds(roles);
        return await _context.RolePermissions
            .Where(p => roleIds.Contains(p.RoleId))
            .Select(p => p.PermissionName)
            .Distinct()
            .Select(p => new Permission(p))
            .ToEquatableSetAsync();
    }

    public async Task<IImmutableSet<Permission>> GetPermissionsForAgent(Agent agent)
    {
        return await RolesByAgent(agent)
            .Join(_context.RolePermissions, u => u.Role, p => p.RoleId, (u, p) => p.PermissionName)
            .Distinct()
            .Select(p => new Permission(p))
            .ToEquatableSetAsync();
    }
}
