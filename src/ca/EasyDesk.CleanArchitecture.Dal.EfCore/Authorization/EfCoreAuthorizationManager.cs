using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class EfCoreAuthorizationManager : IUserPermissionsProvider, IUserRolesManager, IUserRolesProvider, IRolesToPermissionsMapper
{
    private readonly AuthorizationContext _context;

    public EfCoreAuthorizationManager(AuthorizationContext context)
    {
        _context = context;
    }

    private IQueryable<UserRoleModel> RolesByUser(UserId userId) => _context
        .UserRoles
        .Where(u => u.User == userId);

    public async Task<IImmutableSet<Role>> GetRolesForUser(UserInfo userInfo)
    {
        return await RolesByUser(userInfo.UserId)
            .Select(u => new Role(u.Role))
            .ToEquatableSetAsync();
    }

    public async Task GrantRolesToUser(UserId userId, IEnumerable<Role> roles)
    {
        var currentRoleIds = await RolesByUser(userId)
            .Select(r => r.Role)
            .ToListAsync();

        var rolesToBeAdded = RoleIds(roles)
            .Except(currentRoleIds)
            .Select(role => UserRoleModel.Create(userId, role));

        _context.UserRoles.AddRange(rolesToBeAdded);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRolesToUser(UserId userId, IEnumerable<Role> roles)
    {
        var roleIds = RoleIds(roles);
        var rolesToBeRemoved = await RolesByUser(userId)
            .Where(x => roleIds.Contains(x.Role))
            .ToListAsync();

        _context.UserRoles.RemoveRange(rolesToBeRemoved);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllRolesToUser(UserId userId)
    {
        var rolesToBeRemoved = await RolesByUser(userId)
            .ToListAsync();

        _context.UserRoles.RemoveRange(rolesToBeRemoved);
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

    public async Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userInfo)
    {
        return await RolesByUser(userInfo.UserId)
            .Join(_context.RolePermissions, u => u.Role, p => p.RoleId, (u, p) => p.PermissionName)
            .Distinct()
            .Select(p => new Permission(p))
            .ToEquatableSetAsync();
    }
}
