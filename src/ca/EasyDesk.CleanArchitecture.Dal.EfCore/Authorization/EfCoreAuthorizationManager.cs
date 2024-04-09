using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class EfCoreAuthorizationManager : IIdentityRolesManager, IAgentRolesProvider
{
    private readonly AuthorizationContext _context;

    public EfCoreAuthorizationManager(AuthorizationContext context)
    {
        _context = context;
    }

    private IQueryable<IdentityRoleModel> RolesByAgent(Agent agent)
    {
        var predicate = agent
            .Identities
            .Values
            .Select(i => PredicateBuilder.Create<IdentityRoleModel>(r => r.Identity == i.Id && r.Realm == i.Realm))
            .Aggregate(PredicateBuilder.Or);
        return _context
            .IdentityRoles
            .Where(predicate);
    }

    private IQueryable<IdentityRoleModel> RolesByIdentity(Realm realm, IdentityId id)
    {
        return _context
            .IdentityRoles
            .Where(u => u.Identity == id && u.Realm == realm);
    }

    public async Task<IImmutableSet<Role>> GetRolesForAgent(Agent agent)
    {
        return await RolesByAgent(agent)
            .Select(u => new Role(u.Role))
            .ToEquatableSetAsync();
    }

    public async Task GrantRoles(Realm realm, IdentityId id, IEnumerable<Role> roles)
    {
        var currentRoleIds = await RolesByIdentity(realm, id)
            .Select(r => r.Role)
            .ToListAsync();

        var rolesToBeAdded = RoleIds(roles)
            .Except(currentRoleIds)
            .Select(role => IdentityRoleModel.Create(realm, id, role));

        _context.IdentityRoles.AddRange(rolesToBeAdded);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRoles(Realm realm, IdentityId id, IEnumerable<Role> roles)
    {
        var roleIds = RoleIds(roles);
        var rolesToBeRemoved = await RolesByIdentity(realm, id)
            .Where(x => roleIds.Contains(x.Role))
            .ToListAsync();

        _context.IdentityRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllRoles(Realm realm, IdentityId id)
    {
        var rolesToBeRemoved = await RolesByIdentity(realm, id)
            .ToListAsync();

        _context.IdentityRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    private IEnumerable<string> RoleIds(IEnumerable<Role> roles) => roles.Select(x => x.Value);
}
