using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class EfCoreAuthorizationManager : IPermissionsProvider, IUserRolesManager, IRolesToPermissionsMapper
{
    private readonly AuthorizationContext _context;

    public EfCoreAuthorizationManager(AuthorizationContext context)
    {
        _context = context;
    }

    private IQueryable<UserRoleModel> RolesByUser(UserInfo userInfo) => _context
        .UserRoles
        .Where(u => u.UserId == userInfo.UserId);

    public async Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userInfo)
    {
        return await RolesByUser(userInfo)
            .Join(_context.RolePermissions, u => u.RoleId, p => p.RoleId, (u, p) => p.PermissionName)
            .Distinct()
            .Select(p => new Permission(p))
            .ToEquatableSetAsync();
    }

    public async Task<IImmutableSet<Role>> GetRolesForUser(UserInfo userInfo)
    {
        return await RolesByUser(userInfo)
            .Select(u => new Role(u.RoleId))
            .ToEquatableSetAsync();
    }

    public async Task GrantRolesToUser(UserInfo userInfo, IEnumerable<Role> roles)
    {
        var currentRoleIds = await RolesByUser(userInfo)
            .Select(r => r.RoleId)
            .ToListAsync();

        var rolesToBeAdded = RoleIds(roles)
            .Except(currentRoleIds)
            .Select(role => UserRoleModel.Create(userInfo.UserId, role));

        _context.UserRoles.AddRange(rolesToBeAdded);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRolesToUser(UserInfo userInfo, IEnumerable<Role> roles)
    {
        var roleIds = RoleIds(roles);
        var rolesToBeRemoved = await RolesByUser(userInfo)
            .Where(x => roleIds.Contains(x.RoleId))
            .ToListAsync();

        _context.UserRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    private IEnumerable<string> RoleIds(IEnumerable<Role> roles) => roles.Select(r => r.Value);

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
}
